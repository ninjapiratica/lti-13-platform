using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using System.Collections.ObjectModel;

namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    public static class Startup
    {
        public static Lti13PlatformServiceCollection AddLti13PlatformNameRoleProvisioningServices(this Lti13PlatformServiceCollection services)
        {
            services.ExtendLti13Message<IServiceEndpoints, NameRoleProvisioningServiceEndpointsPopulator>();

            return services;
        }

        public static Lti13PlatformEndpointRouteBuilder UseLti13PlatformNameRoleProvisioningServices(this Lti13PlatformEndpointRouteBuilder routeBuilder, Action<Lti13PlatformNameRoleProvisioningServicesEndpointsConfig>? configure = null)
        {
            var config = new Lti13PlatformNameRoleProvisioningServicesEndpointsConfig();
            configure?.Invoke(config);

            routeBuilder.MapGet(config.NamesAndRoleProvisioningServiceUrl,
                async (HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, Service service, CustomReplacements customReplacements, string contextId, string? role, string? rlid, int? limit, int pageIndex = 0, long? since = null) =>
                {
                    const string RESOURCE_LINK_UNAVAILABLE = "resource link unavailable";
                    const string RESOURCE_LINK_UNAVAILABLE_DESCRIPTION = "resource link does not exist in the context";
                    const string RESOURCE_LINK_UNAVAILABLE_URI = "https://www.imsglobal.org/spec/lti-nrps/v2p0#access-restriction";

                    var context = await dataService.GetContextAsync(contextId);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    // Claim: https://purl.imsglobal.org/spec/lti/claim/deployment_id
                    // Claim: ToolId

                    var deployment = (await dataService.GetDeploymentAsync(context.DeploymentId))!;
                    var tool = (await dataService.GetToolAsync(deployment.ToolId))!;

                    var membersResponse = await dataService.GetMembershipsAsync(contextId, pageIndex, limit ?? int.MaxValue, role, rlid);
                    var usersResponse = await dataService.GetUsersAsync(membersResponse.Items.Select(m => m.UserId));

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
                        var oldMembersResponse = await dataService.GetMembershipsAsync(contextId, pageIndex, limit ?? int.MaxValue, role, rlid, new DateTime(since.GetValueOrDefault()));
                        var oldUsersResponse = await dataService.GetUsersAsync(membersResponse.Items.Select(m => m.UserId), new DateTime(since.GetValueOrDefault()));

                        var oldUsers = oldMembersResponse.Items.Join(oldUsersResponse.Items, x => x.UserId, x => x.Id, (m, u) => new { Membership = m, User = u, IsCurrent = false });

                        currentUsers = oldUsers
                            .Concat(currentUsers)
                            .GroupBy(x => x.User.Id)
                            .Where(x => x.Count() == 1 ||
                                x.First().Membership.Status != x.Last().Membership.Status ||
                                x.First().Membership.Roles.OrderBy(y => y).SequenceEqual(x.Last().Membership.Roles.OrderBy(y => y)))
                            .Select(x => x.OrderByDescending(y => y.IsCurrent).First());
                    }

                    var messages = new Dictionary<string, dynamic>();
                    if (!string.IsNullOrWhiteSpace(rlid))
                    {
                        var resourceLink = await dataService.GetContentItemAsync<LtiResourceLinkContentItem>(rlid);

                        if (resourceLink == null || resourceLink?.ContextId != context.Id)
                        {
                            return Results.BadRequest(new { Error = RESOURCE_LINK_UNAVAILABLE, Error_Description = RESOURCE_LINK_UNAVAILABLE_DESCRIPTION, Error_Uri = RESOURCE_LINK_UNAVAILABLE_URI });
                        }

                        foreach (var currentUser in currentUsers)
                        {
                            var dictionary = await customReplacements.ReplaceAsync(new Lti13MessageScope
                            {
                                Tool = tool,
                                Deployment = deployment,
                                Context = context,
                                ResourceLink = resourceLink,
                                User = currentUser.User
                            });

                            if (dictionary != null)
                            {
                                // TODO: populate message: https://www.imsglobal.org/spec/lti-nrps/v2p0#message-section
                                // Custom 'message type' that is similar to resourcelink?
                                // how would we get additional extensions if needed?
                                messages.Add(currentUser.User.Id, dictionary);
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
                        members = currentUsers.Select(x => new
                        {
                            user_id = x.User.Id,
                            roles = x.Membership.Roles,
                            name = x.User.Name,
                            given_name = x.User.GivenName,
                            family_name = x.User.FamilyName,
                            email = x.User.Email,
                            picture = x.User.Picture,
                            status = x.Membership.Status switch
                            {
                                MembershipStatus.Active when x.IsCurrent => "Active", // TODO: make constants
                                MembershipStatus.Inactive when x.IsCurrent => "Inactive",
                                _ => "Deleted"
                            },
                            message = messages.TryGetValue(x.User.Id, out var message) ? message : null
                        })
                    }, contentType: Lti13ContentTypes.MembershipContainer);
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
