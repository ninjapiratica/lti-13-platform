using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Populators;
using System.Collections.ObjectModel;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    public static class Startup
    {
        private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() { TypeInfoResolver = new NameRoleProvisioningMessageTypeResolver(), DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = { new JsonStringEnumConverter() } };

        public static Lti13PlatformBuilder AddLti13PlatformNameRoleProvisioningServices(this Lti13PlatformBuilder platformBuilder, Action<Lti13NameRoleProvisioningServicesBuilder>? config = null)
        {
            var builder = new Lti13NameRoleProvisioningServicesBuilder(platformBuilder)
                .AddMessage(Lti13MessageType.LtiResourceLinkRequest)
                .Extend<Populators.ICustomMessage, Populators.CustomPopulator>();

            config?.Invoke(builder);

            platformBuilder.ExtendLti13Message<IServiceEndpoints, NameRoleProvisioningServiceEndpointsPopulator>();

            return platformBuilder;
        }

        public static Lti13PlatformEndpointRouteBuilder UseLti13PlatformNameRoleProvisioningServices(this Lti13PlatformEndpointRouteBuilder routeBuilder, Action<Lti13PlatformNameRoleProvisioningServicesEndpointsConfig>? configure = null)
        {
            Lti13NameRoleProvisioningServicesBuilder.CreateTypes();

            var config = new Lti13PlatformNameRoleProvisioningServicesEndpointsConfig();
            configure?.Invoke(config);

            routeBuilder.MapGet(config.NamesAndRoleProvisioningServiceUrl,
                async (IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor, IDataService dataService, LinkGenerator linkGenerator, string contextId, string? role, string? rlid, int? limit, int pageIndex = 0, long? since = null) =>
                {
                    const string RESOURCE_LINK_UNAVAILABLE = "resource link unavailable";
                    const string RESOURCE_LINK_UNAVAILABLE_DESCRIPTION = "resource link does not exist in the context";
                    const string RESOURCE_LINK_UNAVAILABLE_URI = "https://www.imsglobal.org/spec/lti-nrps/v2p0#access-restriction";
                    const string ACTIVE = "Active";
                    const string INACTIVE = "Inactive";
                    const string DELETED = "Deleted";

                    var httpContext = httpContextAccessor.HttpContext!;
                    var clientId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

                    // TODO: NULL DEPLOYMENTID
                    var deploymentId = string.Empty;

                    var context = await dataService.GetContextAsync(clientId, deploymentId, contextId);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var tool = clientId != null ? await dataService.GetToolAsync(clientId) : null;
                    if (tool == null)
                    {
                        // TODO: provide results
                        return Results.NotFound();
                    }

                    var membersResponse = await dataService.GetMembershipsAsync(clientId, deploymentId, contextId, pageIndex, limit ?? int.MaxValue, role, rlid);
                    var usersResponse = await dataService.GetUsersAsync(clientId, deploymentId, contextId, membersResponse.Items.Select(m => m.UserId));

                    var links = new Collection<string>();

                    if (membersResponse.TotalItems > limit * (pageIndex + 1))
                    {
                        links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_MEMBERSHIPS, new { contextId, role, rlid, limit, pageIndex = pageIndex + 1 })}>; rel=\"next\"");
                    }

                    links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_MEMBERSHIPS, new { contextId, role, rlid, since = DateTime.UtcNow.Ticks })}>; rel=\"differences\"");

                    httpContext.Response.Headers.Link = new StringValues([.. links]);

                    var currentUsers = membersResponse.Items.Join(usersResponse.Items, x => x.UserId, x => x.Id, (m, u) => new { Membership = m, User = u, IsCurrent = true });

                    if (since.HasValue)
                    {
                        var asOfDate = new DateTime(since.GetValueOrDefault());
                        var oldMembersResponse = await dataService.GetMembershipsAsync(clientId, deploymentId, contextId, pageIndex, limit ?? int.MaxValue, role, rlid, asOfDate);
                        var oldUsersResponse = await dataService.GetUsersAsync(clientId, deploymentId, contextId, membersResponse.Items.Select(m => m.UserId), asOfDate);

                        var oldUsers = oldMembersResponse.Items.Join(oldUsersResponse.Items, x => x.UserId, x => x.Id, (m, u) => new { Membership = m, User = u, IsCurrent = false });

                        currentUsers = oldUsers
                            .Concat(currentUsers)
                            .GroupBy(x => x.User.Id)
                            .Where(x => x.Count() == 1 ||
                                x.First().Membership.Status != x.Last().Membership.Status ||
                                x.First().Membership.Roles.OrderBy(y => y).SequenceEqual(x.Last().Membership.Roles.OrderBy(y => y)))
                            .Select(x => x.OrderByDescending(y => y.IsCurrent).First());
                    }

                    var messages = new Dictionary<string, ICollection<NameRoleProvisioningMessage>>();
                    if (!string.IsNullOrWhiteSpace(rlid))
                    {
                        var resourceLink = await dataService.GetResourceLinkAsync(clientId, deploymentId, contextId, rlid);

                        if (resourceLink == null)
                        {
                            return Results.BadRequest(new { Error = RESOURCE_LINK_UNAVAILABLE, Error_Description = RESOURCE_LINK_UNAVAILABLE_DESCRIPTION, Error_Uri = RESOURCE_LINK_UNAVAILABLE_URI });
                        }

                        // TODO: RESOURCELINK DEPLOYMENTID
                        var deployment = await dataService.GetDeploymentAsync(clientId, deploymentId);

                        if (deployment == null || deployment.ToolId != tool.Id)
                        {
                            return Results.BadRequest(new { Error = RESOURCE_LINK_UNAVAILABLE, Error_Description = RESOURCE_LINK_UNAVAILABLE_DESCRIPTION, Error_Uri = RESOURCE_LINK_UNAVAILABLE_URI });
                        }

                        var messageTypes = Lti13NameRoleProvisioningServicesBuilder.LtiMessageTypes.ToDictionary(mt => mt.Key, mt => serviceProvider.GetKeyedServices<Populator>(mt.Key));

                        foreach (var currentUser in currentUsers)
                        {
                            ICollection<NameRoleProvisioningMessage> userMessages = [];
                            messages.Add(currentUser.User.Id, userMessages);

                            var scope = new Lti13MessageScope(tool, currentUser.User, deployment, context, resourceLink, null);

                            foreach (var messageType in messageTypes)
                            {
                                var message = serviceProvider.GetKeyedService<NameRoleProvisioningMessage>(messageType.Key);

                                if (message != null)
                                {
                                    message.MessageType = messageType.Key.Name;

                                    foreach (var populator in messageType.Value)
                                    {
                                        await populator.PopulateAsync(message, scope);
                                    }

                                    userMessages.Add(message);
                                }
                            }
                        }
                    }

                    return Results.Json(new
                    {
                        id = httpContext.Request.GetDisplayUrl(),
                        context = new
                        {
                            id = context.Id,
                            label = context.Label,
                            title = context.Title
                        },
                        members = currentUsers.Where(u => u.Membership.Roles.Any()).Select(x =>
                        {
                            return new
                            {
                                user_id = x.User.Id,
                                roles = x.Membership.Roles,
                                name = tool.UserPermissions.Name ? x.User.Name : null,
                                given_name = tool.UserPermissions.GivenName ? x.User.GivenName : null,
                                family_name = tool.UserPermissions.FamilyName ? x.User.FamilyName : null,
                                email = tool.UserPermissions.Email ? x.User.Email : null,
                                picture = tool.UserPermissions.Picture ? x.User.Picture : null,
                                status = x.Membership.Status switch
                                {
                                    MembershipStatus.Active when x.IsCurrent => ACTIVE,
                                    MembershipStatus.Inactive when x.IsCurrent => INACTIVE,
                                    _ => DELETED
                                },
                                message = messages.TryGetValue(x.User.Id, out var message) ? message : null
                            };
                        })
                    }, JSON_SERIALIZER_OPTIONS, contentType: Lti13ContentTypes.MembershipContainer);
                })
                .WithName(RouteNames.GET_MEMBERSHIPS)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.MembershipReadOnly);
                });

            return routeBuilder;
        }
    }

    internal static class Lti13ContentTypes
    {
        internal const string MembershipContainer = "application/vnd.ims.lti-nrps.v2.membershipcontainer+json";
    }

    public static class Lti13ServiceScopes
    {
        public const string MembershipReadOnly = "https://purl.imsglobal.org/spec/lti-nrps/scope/contextmembership.readonly";
    }
}
