using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Populators;
using NP.Lti13Platform.Core.Services;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;
using NP.Lti13Platform.NameRoleProvisioningServices.Populators;
using NP.Lti13Platform.NameRoleProvisioningServices.Services;
using System.Collections.ObjectModel;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    public static class Startup
    {
        private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() { TypeInfoResolver = new NameRoleProvisioningMessageTypeResolver(), DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = { new JsonStringEnumConverter() } };
        private static readonly Dictionary<string, MessageType> MessageTypes = [];
        private static readonly Dictionary<MessageType, Type> LtiMessageTypes = [];

        public static Lti13PlatformBuilder AddLti13PlatformNameRoleProvisioningServices(this Lti13PlatformBuilder platformBuilder)
        {
            var builder = platformBuilder
                .ExtendNameRoleProvisioningMessage<Populators.ICustomMessage, Populators.CustomPopulator>(Lti13MessageType.LtiResourceLinkRequest);

            builder.ExtendLti13Message<IServiceEndpoints, ServiceEndpointsPopulator>();

            return builder;
        }

        public static Lti13PlatformBuilder AddDefaultNameRoleProvisioningService(this Lti13PlatformBuilder builder, Action<ServicesConfig>? configure = null)
        {
            configure ??= (x) => { };

            builder.Services.Configure(configure);
            builder.Services.AddTransient<IServiceHelper, ServiceHelper>();
            return builder;
        }

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

        public static IEndpointRouteBuilder UseLti13PlatformNameRoleProvisioningServices(this IEndpointRouteBuilder routeBuilder, Action<EndpointsConfig>? configure = null)
        {
            CreateTypes();

            var config = new EndpointsConfig();
            configure?.Invoke(config);

            routeBuilder.MapGet(config.NamesAndRoleProvisioningServicesUrl,
                async (IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor, ICoreDataService coreDataService, INameRoleProvisioningDataService nrpsDataService, LinkGenerator linkGenerator, string deploymentId, string contextId, string? role, string? rlid, int? limit, int pageIndex = 0, long? since = null) =>
                {
                    const string RESOURCE_LINK_UNAVAILABLE = "resource link unavailable";
                    const string RESOURCE_LINK_UNAVAILABLE_DESCRIPTION = "resource link does not exist in the context";
                    const string RESOURCE_LINK_UNAVAILABLE_URI = "https://www.imsglobal.org/spec/lti-nrps/v2p0#access-restriction";
                    const string ACTIVE = "Active";
                    const string INACTIVE = "Inactive";
                    const string DELETED = "Deleted";

                    var httpContext = httpContextAccessor.HttpContext!;

                    var context = await coreDataService.GetContextAsync(contextId);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var clientId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
                    var tool = await coreDataService.GetToolAsync(clientId);
                    if (tool == null)
                    {
                        return Results.NotFound();
                    }

                    var deployment = await coreDataService.GetDeploymentAsync(deploymentId);
                    if (deployment?.ToolId != tool.Id)
                    {
                        return Results.BadRequest(new { Error = RESOURCE_LINK_UNAVAILABLE, Error_Description = RESOURCE_LINK_UNAVAILABLE_DESCRIPTION, Error_Uri = RESOURCE_LINK_UNAVAILABLE_URI });
                    }

                    var membersResponse = await nrpsDataService.GetMembershipsAsync(deploymentId, contextId, pageIndex, limit ?? int.MaxValue, role, rlid);
                    var usersResponse = await nrpsDataService.GetUsersAsync(membersResponse.Items.Select(m => m.UserId));

                    var links = new Collection<string>();

                    if (membersResponse.TotalItems > limit * (pageIndex + 1))
                    {
                        links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_MEMBERSHIPS, new { deploymentId, contextId, role, rlid, limit, pageIndex = pageIndex + 1 })}>; rel=\"next\"");
                    }

                    links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_MEMBERSHIPS, new { deploymentId, contextId, role, rlid, since = DateTime.UtcNow.Ticks })}>; rel=\"differences\"");

                    httpContext.Response.Headers.Link = new StringValues([.. links]);

                    var currentUsers = membersResponse.Items.Join(usersResponse, x => x.UserId, x => x.Id, (m, u) => new { Membership = m, User = u, IsCurrent = true });

                    if (since.HasValue)
                    {
                        var asOfDate = new DateTime(since.GetValueOrDefault());
                        var oldMembersResponse = await nrpsDataService.GetMembershipsAsync(deploymentId, contextId, pageIndex, limit ?? int.MaxValue, role, rlid, asOfDate);
                        var oldUsersResponse = await nrpsDataService.GetUsersAsync(membersResponse.Items.Select(m => m.UserId), asOfDate);

                        var oldUsers = oldMembersResponse.Items.Join(oldUsersResponse, x => x.UserId, x => x.Id, (m, u) => new { Membership = m, User = u, IsCurrent = false });

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
                        var resourceLink = await coreDataService.GetResourceLinkAsync(rlid);
                        if (resourceLink == null || resourceLink.DeploymentId != deploymentId)
                        {
                            return Results.BadRequest(new { Error = RESOURCE_LINK_UNAVAILABLE, Error_Description = RESOURCE_LINK_UNAVAILABLE_DESCRIPTION, Error_Uri = RESOURCE_LINK_UNAVAILABLE_URI });
                        }

                        var messageTypes = LtiMessageTypes.ToDictionary(mt => mt.Key, mt => serviceProvider.GetKeyedServices<Populator>(mt.Key));

                        foreach (var currentUser in currentUsers)
                        {
                            ICollection<NameRoleProvisioningMessage> userMessages = [];
                            messages.Add(currentUser.User.Id, userMessages);

                            var scope = new Lti13MessageScope(
                                new Lti13UserScope(currentUser.User, ActualUser: null, IsAnonymous: false),
                                tool,
                                deployment,
                                context,
                                resourceLink,
                                MessageHint: null);

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

    internal record MessageType(string Name, HashSet<Type> Interfaces);
}
