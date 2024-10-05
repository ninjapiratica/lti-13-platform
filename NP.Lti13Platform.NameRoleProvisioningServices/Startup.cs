using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.NameRoleProvisioningServices.Populators;
using System.Collections.ObjectModel;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    internal class NameRoleProvisioningMessageTypeResolver : DefaultJsonTypeInfoResolver
    {
        private static readonly HashSet<Type> derivedTypes = [];

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var jsonTypeInfo = base.GetTypeInfo(type, options);

            var baseType = typeof(NameRoleProvisioningMessage);
            if (jsonTypeInfo.Type == baseType)
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    IgnoreUnrecognizedTypeDiscriminators = true,
                };

                foreach (var derivedType in derivedTypes)
                {
                    jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType));
                }
            }

            return jsonTypeInfo;
        }

        public static void AddDerivedType(Type type)
        {
            derivedTypes.Add(type);
        }
    }

    internal record MessageType(string Name, HashSet<Type> Interfaces);

    public class Lti13NameRoleProvisioningServicesMessageBuilder(string messageType, Lti13PlatformBuilder platformBuilder) : Lti13NameRoleProvisioningServicesBuilder(platformBuilder)
    {
        public Lti13NameRoleProvisioningServicesMessageBuilder Extend<T, U>()
            where T : class
            where U : Populator<T>
        {
            base.ExtendMessage<T, U>(messageType);

            return this;
        }
    }

    public class Lti13NameRoleProvisioningServicesBuilder(Lti13PlatformBuilder platformBuilder)
    {
        private static readonly Dictionary<string, MessageType> MessageTypes = [];
        internal static readonly Dictionary<MessageType, Type> LtiMessageTypes = [];

        public Lti13NameRoleProvisioningServicesBuilder ExtendMessage<T, U>(string messageType)
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
                AddMessage(messageType);
                mt = MessageTypes[messageType];
            }

            interfaceTypes.ForEach(t => mt.Interfaces.Add(t));
            platformBuilder.Services.AddKeyedTransient<Populator, U>(messageType);

            return this;
        }

        public Lti13NameRoleProvisioningServicesMessageBuilder AddMessage(string messageType)
        {
            if (!MessageTypes.TryGetValue(messageType, out var mt))
            {
                mt = new MessageType(messageType, []);
                MessageTypes.Add(messageType, mt);
            }

            platformBuilder.Services.AddKeyedTransient(mt, (sp, obj) =>
            {
                return Activator.CreateInstance(LtiMessageTypes[mt])!;
            });

            return new Lti13NameRoleProvisioningServicesMessageBuilder(messageType, platformBuilder);
        }

        static internal void CreateTypes()
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
    }

    public static class Startup
    {
        private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() { TypeInfoResolver = new NameRoleProvisioningMessageTypeResolver(), DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = { new JsonStringEnumConverter() } };

        public static Lti13PlatformBuilder AddLti13PlatformNameRoleProvisioningServices(this Lti13PlatformBuilder platformBuilder, Action<Lti13NameRoleProvisioningServicesBuilder>? config = null)
        {
            var builder = new Lti13NameRoleProvisioningServicesBuilder(platformBuilder)
                .AddMessage(Lti13MessageType.LtiResourceLinkRequest)
                .Extend<ICustomMessage, CustomPopulator>();

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
                    var context = await dataService.GetContextAsync(contextId);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var clientId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                    var tool = clientId != null ? await dataService.GetToolByClientIdAsync(clientId) : null;
                    if (tool == null)
                    {
                        // TODO: provide results
                        return Results.BadRequest();
                    }

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
                        var asOfDate = new DateTime(since.GetValueOrDefault());
                        var oldMembersResponse = await dataService.GetMembershipsAsync(contextId, pageIndex, limit ?? int.MaxValue, role, rlid, asOfDate);
                        var oldUsersResponse = await dataService.GetUsersAsync(membersResponse.Items.Select(m => m.UserId), asOfDate);

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
                        var resourceLink = await dataService.GetContentItemAsync<LtiResourceLinkContentItem>(rlid);

                        if (resourceLink == null || resourceLink?.ContextId != context.Id)
                        {
                            return Results.BadRequest(new { Error = RESOURCE_LINK_UNAVAILABLE, Error_Description = RESOURCE_LINK_UNAVAILABLE_DESCRIPTION, Error_Uri = RESOURCE_LINK_UNAVAILABLE_URI });
                        }

                        var deployment = await dataService.GetDeploymentAsync(resourceLink.DeploymentId);

                        if (deployment == null || deployment.ToolId != tool.Id)
                        {
                            return Results.BadRequest(new { Error = RESOURCE_LINK_UNAVAILABLE, Error_Description = RESOURCE_LINK_UNAVAILABLE_DESCRIPTION, Error_Uri = RESOURCE_LINK_UNAVAILABLE_URI });
                        }

                        var messageTypes = Lti13NameRoleProvisioningServicesBuilder.LtiMessageTypes.ToDictionary(mt => mt.Key, mt => serviceProvider.GetKeyedServices<Populator>(mt.Key));

                        foreach (var currentUser in currentUsers)
                        {
                            ICollection<NameRoleProvisioningMessage> userMessages = [];
                            messages.Add(currentUser.User.Id, userMessages);

                            var scope = new Lti13MessageScope
                            {
                                Tool = tool,
                                Deployment = deployment,
                                Context = context,
                                ResourceLink = resourceLink,
                                User = currentUser.User
                            };

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
