using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Extensions;
using NP.Lti13Platform.Core.MessageClaims;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.OpenApi;
using NP.Lti13Platform.Core.Services;
using NP.Lti13Platform.Core.Utilities;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;
using NP.Lti13Platform.NameRoleProvisioningServices.Constants;
using NP.Lti13Platform.NameRoleProvisioningServices.MessageClaims;
using NP.Lti13Platform.NameRoleProvisioningServices.MessageHandlers;
using NP.Lti13Platform.NameRoleProvisioningServices.Services;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices;

/// <summary>
/// Provides extension methods for configuring LTI 1.3 Name and Role Provisioning Services.
/// </summary>
public static class Endpoints
{
    private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };
    private static readonly Dictionary<string, MessageType> MessageTypes = [];
    private static readonly Dictionary<MessageType, Type> LtiMessageTypes = [];

    /// <summary>
    /// Configures the endpoint for LTI 1.3 Name and Role Provisioning Services.
    /// </summary>
    /// <param name="endpointRouteBuilder">The endpoint route builder.</param>
    /// <param name="configure">Optional function to configure endpoints.</param>
    /// <returns>The endpoint route builder for further configuration.</returns>
    public static IEndpointRouteBuilder UseLti13PlatformNameRoleProvisioningServices(this IEndpointRouteBuilder endpointRouteBuilder, Func<EndpointsConfig, EndpointsConfig>? configure = null)
    {
        const string OpenAPI_Tag = "LTI 1.3 Name and Role Provisioning Services";

        EndpointsConfig config = new();
        config = configure?.Invoke(config) ?? config;

        endpointRouteBuilder.MapGet(config.NamesAndRoleProvisioningServicesUrl,
            async (
                DeploymentId deploymentId,
                ContextId contextId,
                string? role,
                ResourceLinkId? rlid,
                int? limit,
                int? pageIndex,
                long? since,
                IServiceProvider serviceProvider,
                IHttpContextAccessor httpContextAccessor,
                ICoreDataService coreDataService,
                INameRoleProvisioningDataService nrpsDataService,
                IEnumerable<INameRoleProvisioningServicesMessageExtension> messageExtensions,
                IOptionsMonitor<ServicesConfig> config,
                LinkGenerator linkGenerator,
                CancellationToken cancellationToken) =>
            {
                var httpContext = httpContextAccessor.HttpContext!;

                var clientId = new ClientId(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
                var tool = await coreDataService.GetToolAsync(clientId, cancellationToken);
                if (tool == null)
                {
                    return Results.NotFound();
                }

                var deployment = await nrpsDataService.GetDeploymentAsync(deploymentId, cancellationToken);
                if (deployment?.ClientId != tool.ClientId)
                {
                    return Results.NotFound();
                }

                var context = await nrpsDataService.GetContextAsync(contextId, cancellationToken);
                if (context == null)
                {
                    return Results.NotFound();
                }

                ResourceLink? resourceLink = null;
                if (rlid != null && rlid != ResourceLinkId.Empty)
                {
                    resourceLink = await nrpsDataService.GetResourceLinkAsync(rlid.GetValueOrDefault(), cancellationToken);

                    if (resourceLink == null
                        || resourceLink.DeploymentId != deploymentId
                        || resourceLink.ContextId != contextId)
                    {
                        return Results.BadRequest(new Lti13BadRequest
                        {
                            Error = "resource link unavailable",
                            Error_Description = "resource link does not exist in the context",
                            Error_Uri = "https://www.imsglobal.org/spec/lti-nrps/v2p0#access-restriction"
                        });
                    }
                }

                if (!config.CurrentValue.SupportMembershipDifferences
                    && since.HasValue)
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = "membership differences not supported",
                        Error_Description = "the platform does not support membership differences",
                        Error_Uri = "https://www.imsglobal.org/spec/lti-nrps/v2p0#membership-differences"
                    });
                }

                if (config.CurrentValue.SupportMembershipDifferences)
                {
                    httpContext.Response.Headers.Append(
                        nameof(httpContext.Response.Headers.Link),
                        $"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_MEMBERSHIPS, new { deploymentId, contextId, role, rlid, since = DateTime.UtcNow.Ticks })}>; rel=\"differences\"");
                }

                var currentMemberships = await nrpsDataService.GetMembershipsAsync(deploymentId, contextId, role, rlid, cancellationToken: cancellationToken);

                var memberships = currentMemberships
                    .Select(x => (
                        Membership: x,
                        Status: x.Status switch
                        {
                            MembershipStatus.Active => MemberInfoStatus.Active,
                            MembershipStatus.Inactive => MemberInfoStatus.Inactive,
                            _ => MemberInfoStatus.Deleted
                        }));

                // Figure out the membership differences since the provided time
                if (since.HasValue)
                {
                    var oldMemberships = await nrpsDataService.GetMembershipsAsync(deploymentId, contextId, role, rlid, new DateTime(since.Value), cancellationToken);

                    // Old memberships are considered deleted, if currentMemberships exist, it will override
                    memberships = oldMemberships.Select(x => (Membership: x, Status: MemberInfoStatus.Deleted))
                        .Concat(memberships)
                        .GroupBy(x => x.Membership.UserId)
                        .Where(x =>
                            x.Count() == 1
                            || x.First().Membership.Status != x.Last().Membership.Status
                            || !x.First().Membership.Roles.OrderBy(y => y).SequenceEqual(x.Last().Membership.Roles.OrderBy(y => y)))
                        .Select(x =>
                        {
                            var nonDeleted = x.FirstOrDefault(y => y.Status != MemberInfoStatus.Deleted);
                            return nonDeleted == default ? x.First() : nonDeleted;
                        });
                }

                // Apply pagination
                if (limit.HasValue)
                {
                    if (memberships.Count() > limit * (pageIndex.GetValueOrDefault() + 1))
                    {
                        httpContext.Response.Headers.Append(
                            nameof(httpContext.Response.Headers.Link),
                            $"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_MEMBERSHIPS, new { deploymentId, contextId, role, rlid, limit, pageIndex = pageIndex.GetValueOrDefault() + 1 })}>; rel=\"next\"");
                    }

                    memberships = memberships
                        .OrderBy(x => x.Membership.UserId)
                        .Skip(limit.GetValueOrDefault() * pageIndex.GetValueOrDefault())
                        .Take(limit.GetValueOrDefault());
                }

                // Append users and permissions to memberships
                var users = await nrpsDataService.GetUsersAsync(memberships.Select(u => u.Membership.UserId), cancellationToken);
                var userPermissions = await nrpsDataService.GetUserPermissionsAsync(deploymentId, contextId, memberships.Select(x => x.Membership.UserId), cancellationToken);

                var membershipUsers = memberships
                    .Join(
                        users,
                        x => x.Membership.UserId,
                        x => x.Id,
                        (m, u) => (User: u, m.Membership, m.Status))
                    .Join(
                        userPermissions,
                        x => x.Membership.UserId,
                        x => x.UserId,
                        (u, p) => (u.User, u.Membership, u.Status, UserPermissions: p));

                // Create LTI messages for each user if resource link is provided
                var messages = new Dictionary<UserId, IEnumerable<object>>();
                if (resourceLink != null)
                {
                    /*
                     * When queried in the context of a Resource Link, an additional message section is added per member. 
                     * This element must contain any context or resource link specific message parameters, including any extension or custom parameters, 
                     * which would be included in the message from the specified Resource Link and which contain data specific to the member.
                     * The parameters must be included using the LTI 1.3 claims format defined in [LTI-13].
                     */

                    var usersWithMessages = membershipUsers
                        .Where(x => x.Status != MemberInfoStatus.Deleted)
                        .ToList();

                    var customPermissions = await nrpsDataService.GetCustomPermissionsAsync(deploymentId, contextId, usersWithMessages.Select(x => x.User.Id), cancellationToken);
                    var attempts = await nrpsDataService.GetAttemptsAsync(resourceLink.Id, usersWithMessages.Select(x => x.User.Id), cancellationToken);

                    var lineItems = await nrpsDataService.GetLineItemsAsync(resourceLink.Id, pageIndex: 0, limit: 1, cancellationToken);

                    var grades = lineItems.TotalItems == 1
                        ? await nrpsDataService.GetGradesAsync(lineItems.Items.First().Id, usersWithMessages.Select(x => x.User.Id), cancellationToken)
                        : usersWithMessages.Select(m => (Grade?)null);

                    // Extract extension message interfaces upfront
                    var extensionMessageInterfaces = messageExtensions
                        .Select(e => e.GetType())
                        .SelectMany(t => t.GetInterfaces())
                        .Where(i => i.IsGenericType
                            && i.GetGenericTypeDefinition() == typeof(INameRoleProvisioningServicesMessageExtension<>))
                        .Select(i => i.GetGenericArguments()[0])
                        .Distinct()
                        .ToList();

                    // Create dynamic type if extensions exist, otherwise use base type
                    var messageType = extensionMessageInterfaces.Count != 0
                        ? DynamicTypeBuilder.CreateTypeImplementingInterfaces<NameRoleProvisioningLtiResourceLinkMessage>(extensionMessageInterfaces)
                        : typeof(NameRoleProvisioningLtiResourceLinkMessage);

                    var userMessages = usersWithMessages
                        .Zip(customPermissions, (userWithMessage, customPermissions) => (userWithMessage.User, userWithMessage.Membership, CustomPermissions: customPermissions))
                        .Zip(attempts, (zip, attempt) => (zip.User, zip.Membership, zip.CustomPermissions, Attempt: attempt))
                        .Zip(grades, (zip, grade) => (zip.User, zip.Membership, zip.CustomPermissions, zip.Attempt, Grade: grade))
                        .Select(zip =>
                        {
                            // Create instance of message (dynamic or base type)
                            var lti13Message = Activator.CreateInstance(messageType)
                                ?? throw new InvalidOperationException("Failed to create message instance.");

                            if (lti13Message is not NameRoleProvisioningLtiResourceLinkMessage message)
                            {
                                throw new InvalidOperationException("Failed to create NameRoleProvisioningLtiResourceLinkMessage instance.");
                            }

                            return (
                                UserId: zip.User.Id,
                                Message: message
                                    .WithCustomClaims(
                                        zip.CustomPermissions,
                                        tool,
                                        deployment,
                                        resourceLink,
                                        zip.Membership,
                                        zip.User,
                                        zip.Attempt,
                                        zip.Grade)
                            );
                        })
                        .ToList();

                    if (extensionMessageInterfaces.Count != 0)
                    {
                        // Create a mapping of user IDs to messages for the extensions
                        var messageDict = userMessages.ToDictionary(x => x.UserId, x => (object)x.Message);
                        
                        await InvokeExtensionsAsync(messageDict, tool, resourceLink, messageExtensions, cancellationToken);

                        messages = userMessages.ToDictionary(x => x.UserId, x => (IEnumerable<object>)[x.Message]);
                    }
                    else
                    {
                        messages = userMessages.ToDictionary(x => x.UserId, x => (IEnumerable<object>)[x.Message]);
                    }
                }

                return Results.Json(new MembershipContainer
                {
                    Id = httpContext.Request.GetDisplayUrl(),
                    Context = new MembershipContext
                    {
                        Id = context.Id,
                        Label = context.Label,
                        Title = context.Title
                    },
                    Members = membershipUsers.Select(x =>
                    {
                        return new MemberInfo
                        {
                            UserId = x.User.Id,
                            Roles = x.Membership.Roles,
                            Name = x.UserPermissions.Name ? x.User.Name : null,
                            GivenName = x.UserPermissions.GivenName ? x.User.GivenName : null,
                            FamilyName = x.UserPermissions.FamilyName ? x.User.FamilyName : null,
                            Email = x.UserPermissions.Email ? x.User.Email : null,
                            Picture = x.UserPermissions.Picture ? x.User.Picture : null,
                            Status = x.Status,
                            Message = messages.TryGetValue(x.User.Id, out var userMessages) ? userMessages : null
                        };
                    })
                }, JSON_SERIALIZER_OPTIONS, contentType: Lti13ContentTypes.MembershipContainer);
            })
            .WithName(RouteNames.GET_MEMBERSHIPS)
            .RequireAuthorization(policy =>
            {
                policy.AddAuthenticationSchemes(ServicesAuthHandler.SchemeName);
                policy.RequireRole(Lti13ServiceScopes.MembershipReadOnly);
            })
            .Produces<MembershipContainer>(contentType: MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<Lti13BadRequest>(StatusCodes.Status400BadRequest)
            .WithGroupName(Lti13OpenApi.GroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Gets the memberships within a context.")
            .WithDescription("Gets the memberships for a context. Can be filtered by role or resourceLinkId (rlid). It is a paginated request so page size and index may be provided. Pagination information (next, previous, etc) will be returned as headers. This endpoint can also be used to get changes in membership since a specified time. If rlid is provided, messages may be returned with the memberships.");

        return endpointRouteBuilder;
    }

    private static async Task InvokeExtensionsAsync(
        IDictionary<UserId, object> messageDict,
        Tool tool,
        ResourceLink resourceLink,
        IEnumerable<INameRoleProvisioningServicesMessageExtension> extensions,
        CancellationToken cancellationToken)
    {
        var extensionTasks = extensions
            .Select(extension => extension.ExtendMessagesAsync(messageDict, tool, resourceLink, cancellationToken))
            .ToList();

        if (extensionTasks.Count > 0)
        {
            await Task.WhenAll(extensionTasks);
        }
    }
}

internal record MessageType(string Name, HashSet<Type> Interfaces);

internal record MembershipContainer
{
    public required string Id { get; set; }
    public required MembershipContext Context { get; set; }
    public required IEnumerable<MemberInfo> Members { get; set; }
}

internal record MembershipContext
{
    public required ContextId Id { get; set; }
    public string? Label { get; set; }
    public string? Title { get; set; }
}

internal record MemberInfo
{
    [JsonPropertyName("user_id")]
    public required UserId UserId { get; set; }
    public required IEnumerable<string> Roles { get; set; }
    public string? Name { get; set; }
    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }
    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }
    public string? Email { get; set; }
    public Uri? Picture { get; set; }
    public required MemberInfoStatus Status { get; set; }
    public IEnumerable<object>? Message { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<MemberInfoStatus>))]
internal enum MemberInfoStatus
{
    Active,
    Inactive,
    Deleted
}

internal record LtiNrpsResourceLinkMessage : INrpsCustomClaims
{
    public IDictionary<string, string>? Custom { get; set; }
}