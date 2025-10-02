using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Populators;
using NP.Lti13Platform.Core.Services;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;
using NP.Lti13Platform.NameRoleProvisioningServices.Populators;
using NP.Lti13Platform.NameRoleProvisioningServices.Services;
using System.Collections.ObjectModel;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices;

/// <summary>
/// Provides extension methods for configuring LTI 1.3 Name and Role Provisioning Services.
/// </summary>
public static class Startup
{
    private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new NameRoleProvisioningMessageTypeResolver(),
        Converters = { new JsonStringEnumConverter() },
    };
    private static readonly Dictionary<string, MessageType> MessageTypes = [];
    private static readonly Dictionary<MessageType, Type> LtiMessageTypes = [];

    /// <summary>
    /// Adds LTI 1.3 Name and Role Provisioning Services to the platform.
    /// </summary>
    /// <param name="platformBuilder">The LTI 1.3 platform builder.</param>
    /// <returns>The LTI 1.3 platform builder for further configuration.</returns>
    public static Lti13PlatformBuilder AddLti13PlatformNameRoleProvisioningServices(this Lti13PlatformBuilder platformBuilder)
    {
        var builder = platformBuilder
            .ExtendNameRoleProvisioningMessage<Populators.ICustomMessage, Populators.CustomPopulator>(Lti13MessageType.LtiResourceLinkRequest);

        builder.ExtendLti13Message<IServiceEndpoints, ServiceEndpointsPopulator>();

        builder.Services.AddOptions<ServicesConfig>().BindConfiguration("Lti13Platform:NameRoleProvisioningServices");
        builder.Services.TryAddSingleton<ILti13NameRoleProvisioningConfigService, DefaultNameRoleProvisioningConfigService>();

        return builder;
    }

    /// <summary>
    /// Configures a custom implementation of the Name and Role Provisioning Data Service.
    /// </summary>
    /// <typeparam name="T">The type implementing ILti13NameRoleProvisioningDataService.</typeparam>
    /// <param name="builder">The LTI 1.3 platform builder.</param>
    /// <param name="serviceLifetime">The lifetime of the service in the dependency injection container.</param>
    /// <returns>The LTI 1.3 platform builder for further configuration.</returns>
    public static Lti13PlatformBuilder WithLti13NameRoleProvisioningDataService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13NameRoleProvisioningDataService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13NameRoleProvisioningDataService), typeof(T), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Configures a custom implementation of the Name and Role Provisioning Config Service.
    /// </summary>
    /// <typeparam name="T">The type implementing ILti13NameRoleProvisioningConfigService.</typeparam>
    /// <param name="builder">The LTI 1.3 platform builder.</param>
    /// <param name="serviceLifetime">The lifetime of the service in the dependency injection container.</param>
    /// <returns>The LTI 1.3 platform builder for further configuration.</returns>
    public static Lti13PlatformBuilder WithLti13NameRoleProvisioningConfigService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13NameRoleProvisioningConfigService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13NameRoleProvisioningConfigService), typeof(T), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Extends Name and Role Provisioning message with a custom interface and populator.
    /// </summary>
    /// <typeparam name="T">The interface type that will be added to the message.</typeparam>
    /// <typeparam name="U">The populator type that will populate the interface.</typeparam>
    /// <param name="builder">The LTI 1.3 platform builder.</param>
    /// <param name="messageType">The message type to extend.</param>
    /// <returns>The LTI 1.3 platform builder for further configuration.</returns>
    /// <exception cref="Exception">Thrown when T is not an interface or when it contains methods.</exception>
    public static Lti13PlatformBuilder ExtendNameRoleProvisioningMessage<T, U>(this Lti13PlatformBuilder builder, string messageType)
        where T : class
        where U : Populator<T>
    {
        var tType = typeof(T);
        List<Type> interfaceTypes = [tType, .. tType.GetInterfaces()];

        foreach (var interfaceType in interfaceTypes)
        {
            if (!interfaceType.IsInterface)
            {
                throw new Exception("T must be an interface");
            }

            if (interfaceType.GetMethods().Any(m => !m.IsSpecialName))
            {
                throw new Exception("Interfaces may only have properties.");
            }
        }

        if (!MessageTypes.TryGetValue(messageType, out var mt))
        {
            mt = new MessageType(messageType, []);
            MessageTypes.Add(messageType, mt);

            builder.Services.TryAddKeyedTransient(mt, (sp, obj) =>
            {
                return Activator.CreateInstance(LtiMessageTypes[mt])!;
            });
        }

        interfaceTypes.ForEach(t => mt.Interfaces.Add(t));
        builder.Services.AddKeyedTransient<Populator, U>(mt);

        return builder;
    }

    private static void CreateTypes()
    {
        if (LtiMessageTypes.Count == 0)
        {
            foreach (var messageType in MessageTypes.Select(mt => mt.Value))
            {
                var type = TypeGenerator.CreateType(messageType.Name, messageType.Interfaces, typeof(NameRoleProvisioningMessage));
                NameRoleProvisioningMessageTypeResolver.AddDerivedType(type);
                LtiMessageTypes.TryAdd(messageType, type);
            }
        }
    }

    /// <summary>
    /// Configures the endpoint for LTI 1.3 Name and Role Provisioning Services.
    /// </summary>
    /// <param name="endpointRouteBuilder">The endpoint route builder.</param>
    /// <param name="configure">Optional function to configure endpoints.</param>
    /// <returns>The endpoint route builder for further configuration.</returns>
    public static IEndpointRouteBuilder UseLti13PlatformNameRoleProvisioningServices(this IEndpointRouteBuilder endpointRouteBuilder, Func<EndpointsConfig, EndpointsConfig>? configure = null)
    {
        const string OpenAPI_Tag = "LTI 1.3 Name and Role Provisioning Services";

        CreateTypes();

        EndpointsConfig config = new();
        config = configure?.Invoke(config) ?? config;

        endpointRouteBuilder.MapGet(config.NamesAndRoleProvisioningServicesUrl,
            async (IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor, ILti13CoreDataService coreDataService, ILti13NameRoleProvisioningDataService nrpsDataService, LinkGenerator linkGenerator, string deploymentId, string contextId, string? role, string? rlid, int? limit, int? pageIndex, long? since, CancellationToken cancellationToken) =>
            {
                var httpContext = httpContextAccessor.HttpContext!;
                pageIndex ??= 0;

                var context = await coreDataService.GetContextAsync(contextId, cancellationToken);
                if (context == null)
                {
                    return Results.NotFound();
                }

                var clientId = new ClientId(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
                var tool = await coreDataService.GetToolAsync(clientId, cancellationToken);
                if (tool == null)
                {
                    return Results.NotFound();
                }

                var deployment = await coreDataService.GetDeploymentAsync(deploymentId, cancellationToken);
                if (deployment?.ClientId != tool.ClientId)
                {
                    return Results.NotFound();
                }

                var membersResponse = await nrpsDataService.GetMembershipsAsync(deploymentId, contextId, pageIndex.Value, limit ?? int.MaxValue, role, rlid, cancellationToken: cancellationToken);

                var links = new Collection<string>();

                if (membersResponse.TotalItems > limit * (pageIndex + 1))
                {
                    links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_MEMBERSHIPS, new { deploymentId, contextId, role, rlid, limit, pageIndex = pageIndex + 1 })}>; rel=\"next\"");
                }

                links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_MEMBERSHIPS, new { deploymentId, contextId, role, rlid, since = DateTime.UtcNow.Ticks })}>; rel=\"differences\"");

                httpContext.Response.Headers.Link = new StringValues([.. links]);

                var currentUsers = membersResponse.Items.Select(x => (x.Membership, x.User, IsCurrent: true));

                if (since.HasValue)
                {
                    var asOfDate = new DateTime(since.GetValueOrDefault());
                    var oldMembersResponse = await nrpsDataService.GetMembershipsAsync(deploymentId, contextId, pageIndex.Value, limit ?? int.MaxValue, role, rlid, asOfDate, cancellationToken);

                    var oldUsers = oldMembersResponse.Items.Select(x => (x.Membership, x.User, IsCurrent: false));

                    currentUsers = oldUsers
                        .Concat(currentUsers)
                        .GroupBy(x => x.User.Id)
                        .Where(x => x.Count() == 1 ||
                            x.First().Membership.Status != x.Last().Membership.Status ||
                            x.First().Membership.Roles.OrderBy(y => y).SequenceEqual(x.Last().Membership.Roles.OrderBy(y => y)))
                        .Select(x => x.OrderByDescending(y => y.IsCurrent).First());
                }

                var messages = new Dictionary<string, IEnumerable<NameRoleProvisioningMessage>>();
                if (!string.IsNullOrWhiteSpace(rlid))
                {
                    var resourceLink = await coreDataService.GetResourceLinkAsync(rlid, cancellationToken);
                    if (resourceLink == null || resourceLink.DeploymentId != deploymentId)
                    {
                        return Results.BadRequest(new LtiBadRequest { Error = "resource link unavailable", Error_Description = "resource link does not exist in the context", Error_Uri = "https://www.imsglobal.org/spec/lti-nrps/v2p0#access-restriction" });
                    }

                    IEnumerable<(MessageType MessageType, NameRoleProvisioningMessage Message, MessageScope Scope)> GetUserMessages(User user)
                    {
                        var scope = new MessageScope(
                            new UserScope(user, ActualUser: null, IsAnonymous: false),
                            tool,
                            deployment,
                            context,
                            resourceLink,
                            MessageHint: null);

                        foreach (var messageType in LtiMessageTypes)
                        {
                            var message = serviceProvider.GetKeyedService<NameRoleProvisioningMessage>(messageType.Key);

                            if (message != null)
                            {
                                message.MessageType = messageType.Key.Name;
                                yield return (messageType.Key, message, scope);
                            }
                        }
                    }

                    var userMessages = currentUsers.SelectMany(currentUser => GetUserMessages(currentUser.User));

                    var tasks = userMessages.Select(async userMessage =>
                    {
                        var populators = serviceProvider.GetKeyedServices<Populator>(userMessage.MessageType.Name);

                        foreach (var populator in populators)
                        {
                            await populator.PopulateAsync(userMessage.Message, userMessage.Scope, cancellationToken);
                        }

                        return (userMessage.Scope.UserScope.User.Id, userMessage.Message);
                    });

                    messages = (await Task.WhenAll(tasks)).GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.Select(y => y.Message));
                }

                var usersWithRoles = currentUsers.Where(u => u.Membership.Roles.Any());

                var userPermissions = await nrpsDataService.GetUserPermissionsAsync(deploymentId, contextId, usersWithRoles.Select(u => u.User.Id), cancellationToken);

                var users = usersWithRoles.Join(userPermissions, u => u.User.Id, p => p.UserId, (u, p) => (u.User, u.Membership, UserPermissions: p, u.IsCurrent));

                return Results.Json(new MembershipContainer
                {
                    Id = httpContext.Request.GetDisplayUrl(),
                    Context = new MembershipContext
                    {
                        Id = context.Id,
                        Label = context.Label,
                        Title = context.Title
                    },
                    Members = users.Select(x =>
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
                            Status = x.Membership.Status switch
                            {
                                MembershipStatus.Active when x.IsCurrent => MemberInfoStatus.Active,
                                MembershipStatus.Inactive when x.IsCurrent => MemberInfoStatus.Inactive,
                                _ => MemberInfoStatus.Deleted
                            },
                            Message = messages.TryGetValue(x.User.Id, out var message) ? message : null
                        };
                    })
                }, JSON_SERIALIZER_OPTIONS, contentType: Lti13ContentTypes.MembershipContainer);
            })
            .WithName(RouteNames.GET_MEMBERSHIPS)
            .RequireAuthorization(policy =>
            {
                policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                policy.RequireRole(Lti13ServiceScopes.MembershipReadOnly);
            })
            .Produces<MembershipContainer>(contentType: MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<LtiBadRequest>(StatusCodes.Status400BadRequest)
            .WithGroupName(OpenApi.GroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Gets the memberships within a context.")
            .WithDescription("Gets the memberships for a context. Can be filtered by role or resourceLinkId (rlid). It is a paginated request so page size and index may be provided. Pagination information (next, previous, etc) will be returned as headers. This endpoint can also be used to get changes in membership since a specified time. If rlid is provided, messages may be returned with the memberships.");

        return endpointRouteBuilder;
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
    public required string Id { get; set; }
    public string? Label { get; set; }
    public string? Title { get; set; }
}

internal record MemberInfo
{
    [JsonPropertyName("user_id")]
    public required string UserId { get; set; }
    public required IEnumerable<string> Roles { get; set; }
    public string? Name { get; set; }
    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }
    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }
    public string? Email { get; set; }
    public Uri? Picture { get; set; }
    public required MemberInfoStatus Status { get; set; }
    public IEnumerable<NameRoleProvisioningMessage>? Message { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<MemberInfoStatus>))]
internal enum MemberInfoStatus
{
    Active,
    Inactive,
    Deleted
}