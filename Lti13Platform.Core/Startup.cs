using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Populators;
using System.Collections;
using System.Collections.ObjectModel;
using System.Net.Mime;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Web;

namespace NP.Lti13Platform.Core
{
    internal class LtiMessageTypeResolver : DefaultJsonTypeInfoResolver
    {
        private static readonly HashSet<Type> derivedTypes = [];

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var jsonTypeInfo = base.GetTypeInfo(type, options);

            var baseType = typeof(LtiMessage);
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

    public class Lti13PlatformServiceCollection(IServiceCollection baseCollection) : IServiceCollection
    {
        private static readonly ModuleBuilder dynamicModule = AssemblyBuilder
            .DefineDynamicAssembly(new AssemblyName(Assembly.GetExecutingAssembly().GetName().Name + ".DynamicAssembly"), AssemblyBuilderAccess.Run)
            .DefineDynamicModule("DynamicModule");

        private static readonly HashSet<Type> GlobalInterfaces = [];
        private static readonly HashSet<Type> GlobalPopulators = [];
        private static readonly Dictionary<string, MessageType> MessageTypes = [];
        protected static readonly Dictionary<string, Type> LtiMessageTypes = [];

        public Lti13PlatformServiceCollection ExtendLti13Message<T, U>(string? messageType = null)
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

            if (string.IsNullOrWhiteSpace(messageType))
            {
                var uType = typeof(U);
                GlobalPopulators.Add(uType);

                interfaceTypes.ForEach(t => GlobalInterfaces.Add(t));

                foreach (var mt in MessageTypes.Values)
                {
                    interfaceTypes.ForEach(t => mt.Interfaces.Add(t));

                    baseCollection.AddKeyedTransient<Populator, U>(mt.Name);
                };
            }
            else
            {
                if (!MessageTypes.TryGetValue(messageType, out var mt))
                {
                    AddMessageHandler(messageType);
                    mt = MessageTypes[messageType];
                }

                interfaceTypes.ForEach(t => mt.Interfaces.Add(t));
                baseCollection.AddKeyedTransient<Populator, U>(messageType);
            }

            return this;
        }

        public Lti13PlatformServiceCollectionMessageHandler AddMessageHandler(string messageType)
        {
            MessageTypes.TryAdd(messageType, new MessageType(messageType, [.. GlobalInterfaces]));

            foreach (var globalPopulator in GlobalPopulators)
            {
                baseCollection.AddKeyedTransient(typeof(Populator), messageType, globalPopulator);
            }

            baseCollection.AddKeyedTransient<LtiMessage>(messageType, (sp, obj) =>
            {
                if (LtiMessageTypes.Count == 0)
                {
                    foreach (var messageType in MessageTypes.Select(mt => mt.Value))
                    {
                        var typeBuilder = dynamicModule.DefineType("Dynamic" + Regex.Replace(messageType.Name.Trim(), "[^a-zA-Z0-9]", "_"), TypeAttributes.Public, typeof(LtiMessage));

                        foreach (var iFace in messageType.Interfaces)
                        {
                            typeBuilder.AddInterfaceImplementation(iFace);

                            foreach (var propertyInfo in iFace.GetProperties())
                            {
                                var fieldBuilder = typeBuilder.DefineField("_" + propertyInfo.Name.ToLower(), propertyInfo.PropertyType, FieldAttributes.Private);
                                var propertyBuilder = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);

                                foreach (var customAttribute in propertyInfo.CustomAttributes)
                                {
                                    var propertyArguments = customAttribute.NamedArguments.Where(a => !a.IsField).Select(a => new { PropertyInfo = (PropertyInfo)a.MemberInfo, a.TypedValue.Value }).ToArray();
                                    var fieldArguments = customAttribute.NamedArguments.Where(a => a.IsField).Select(a => new { FieldInfo = (FieldInfo)a.MemberInfo, a.TypedValue.Value }).ToArray();

                                    var constructorArgs = customAttribute.ConstructorArguments.Select(a => a.Value is ReadOnlyCollection<CustomAttributeTypedArgument> collection ? collection.Select(c => c.Value).ToArray() : a.Value).ToArray();
                                    var propertyArgs = propertyArguments.Select(a => a.PropertyInfo).ToArray();
                                    var propertyValues = propertyArguments.Select(a => a.Value).ToArray();
                                    var fieldArgs = fieldArguments.Select(a => a.FieldInfo).ToArray();
                                    var fieldValues = fieldArguments.Select(a => a.Value).ToArray();

                                    var customBuilder = new CustomAttributeBuilder(
                                        customAttribute.Constructor,
                                        constructorArgs,
                                        propertyArgs,
                                        propertyValues,
                                        fieldArgs,
                                        fieldValues);

                                    propertyBuilder.SetCustomAttribute(customBuilder);
                                }

                                var getter = typeBuilder.DefineMethod("get_" + propertyInfo.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, propertyInfo.PropertyType, Type.EmptyTypes);
                                var getGenerator = getter.GetILGenerator();
                                getGenerator.Emit(OpCodes.Ldarg_0);
                                getGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
                                getGenerator.Emit(OpCodes.Ret);
                                propertyBuilder.SetGetMethod(getter);
                                var getMethod = propertyInfo.GetGetMethod();
                                if (getMethod != null)
                                {
                                    typeBuilder.DefineMethodOverride(getter, getMethod);
                                }

                                var setter = typeBuilder.DefineMethod("set_" + propertyInfo.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, null, [propertyInfo.PropertyType]);
                                var setGenerator = setter.GetILGenerator();
                                setGenerator.Emit(OpCodes.Ldarg_0);
                                setGenerator.Emit(OpCodes.Ldarg_1);
                                setGenerator.Emit(OpCodes.Stfld, fieldBuilder);
                                setGenerator.Emit(OpCodes.Ret);
                                propertyBuilder.SetSetMethod(setter);
                                var setMethod = propertyInfo.GetSetMethod();
                                if (setMethod != null)
                                {
                                    typeBuilder.DefineMethodOverride(setter, setMethod);
                                }
                            }
                        }

                        var type = typeBuilder.CreateType();
                        LtiMessageTypeResolver.AddDerivedType(type);
                        LtiMessageTypes.TryAdd(messageType.Name, type);
                    }
                }

                return (LtiMessage)Activator.CreateInstance(LtiMessageTypes[messageType])!;
            });

            return new Lti13PlatformServiceCollectionMessageHandler(baseCollection, messageType);
        }

        public ServiceDescriptor this[int index] { get => baseCollection[index]; set => baseCollection[index] = value; }

        public int Count => baseCollection.Count;

        public bool IsReadOnly => baseCollection.IsReadOnly;

        public void Add(ServiceDescriptor item) => baseCollection.Add(item);

        public void Clear() => baseCollection.Clear();

        public bool Contains(ServiceDescriptor item) => baseCollection.Contains(item);

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => baseCollection.CopyTo(array, arrayIndex);

        public IEnumerator<ServiceDescriptor> GetEnumerator() => baseCollection.GetEnumerator();

        public int IndexOf(ServiceDescriptor item) => baseCollection.IndexOf(item);

        public void Insert(int index, ServiceDescriptor item) => baseCollection.Insert(index, item);

        public bool Remove(ServiceDescriptor item) => baseCollection.Remove(item);

        public void RemoveAt(int index) => baseCollection.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)baseCollection).GetEnumerator();
    }

    public class Lti13PlatformServiceCollectionMessageHandler(IServiceCollection baseCollection, string messageType) : Lti13PlatformServiceCollection(baseCollection)
    {
        public Lti13PlatformServiceCollectionMessageHandler ExtendLti13Message<T, U>()
            where T : class
            where U : Populator<T>
        {
            base.ExtendLti13Message<T, U>(messageType);

            return this;
        }
    }

    public class Lti13PlatformEndpointRouteBuilder(IEndpointRouteBuilder builder) : IEndpointRouteBuilder
    {
        public IServiceProvider ServiceProvider => builder.ServiceProvider;

        public ICollection<EndpointDataSource> DataSources => builder.DataSources;

        public IApplicationBuilder CreateApplicationBuilder() => builder.CreateApplicationBuilder();
    }

    public class Lti13MessageScope
    {
        public required Tool Tool { get; set; }

        public required User User { get; set; }

        public required Deployment Deployment { get; set; }

        public Context? Context { get; set; }

        public LtiResourceLinkContentItem? ResourceLink { get; set; }

        public string? MessageHint { get; set; }
    }

    public abstract class Populator
    {
        public abstract Task Populate(object obj, Lti13MessageScope scope);
    }

    public abstract class Populator<T> : Populator
    {
        public abstract Task Populate(T obj, Lti13MessageScope scope);

        public override async Task Populate(object obj, Lti13MessageScope scope)
        {
            await Populate((T)obj, scope);
        }
    }

    // Copied from HttpContextAccessor (with VS_TUNNEL_URL modification)
    public class DevTunnelHttpContextAccessor : IHttpContextAccessor
    {
        private static readonly AsyncLocal<HttpContextHolder> _httpContextCurrent = new AsyncLocal<HttpContextHolder>();

        public HttpContext? HttpContext
        {
            get
            {
                return _httpContextCurrent.Value?.Context;
            }
            set
            {
                var holder = _httpContextCurrent.Value;
                if (holder != null)
                {
                    // Clear current HttpContext trapped in the AsyncLocals, as its done.
                    holder.Context = null;
                }

                if (value != null)
                {
                    var devTunnel = Environment.GetEnvironmentVariable("VS_TUNNEL_URL");
                    if (!string.IsNullOrWhiteSpace(devTunnel))
                    {
                        value.Request.Host = new HostString(new Uri(devTunnel).Host);
                    }

                    // Use an object indirection to hold the HttpContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _httpContextCurrent.Value = new HttpContextHolder { Context = value };
                }
            }
        }

        private sealed class HttpContextHolder
        {
            public HttpContext? Context;
        }
    }

    public static class Startup
    {
        private static readonly JsonSerializerOptions JSON_OPTIONS = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    (typeInfo) =>
                    {
                        foreach(var prop in typeInfo.Properties.Where(p => p.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(p.PropertyType)))
                        {
                            prop.ShouldSerialize = (obj, val) => val is IEnumerable e && e.GetEnumerator().MoveNext();
                        }
                    }
                }
            }
        };
        private static readonly CryptoProviderFactory CRYPTO_PROVIDER_FACTORY = new() { CacheSignatureProviders = false };
        private static readonly JsonSerializerOptions LTI_MESSAGE_JSON_SERIALIZER_OPTIONS = new() { TypeInfoResolver = new LtiMessageTypeResolver(), DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = { new JsonStringEnumConverter() } };

        public static Lti13PlatformServiceCollection AddLti13PlatformCore(this IServiceCollection serviceCollection, Action<Lti13PlatformConfig> configure)
        {
            var services = new Lti13PlatformServiceCollection(serviceCollection);

            services.Configure(configure);

            services.AddTransient<Service>();
            services.AddTransient<CustomReplacements>();
            services.AddTransient<IPlatformService, PlatformService>();

            services.AddMessageHandler(Lti13MessageType.LtiResourceLinkRequest)
                .ExtendLti13Message<IResourceLinkMessage, ResourceLinkPopulator>()
                .ExtendLti13Message<IPlatformMessage, PlatformPopulator>()
                .ExtendLti13Message<IContextMessage, ContextPopulator>()
                .ExtendLti13Message<ICustomMessage, CustomPopulator>()
                .ExtendLti13Message<IRolesMessage, RolesPopulator>();

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, LtiServicesAuthHandler>(LtiServicesAuthHandler.SchemeName, null);

            services.AddHttpContextAccessor();

            return services;
        }

        public static T AddDevTunnelHttpContextAccessor<T>(this T serviceCollection) where T : IServiceCollection
        {
            serviceCollection.RemoveAll<IHttpContextAccessor>();
            serviceCollection.AddSingleton<IHttpContextAccessor, DevTunnelHttpContextAccessor>();

            return serviceCollection;
        }

        public static Lti13PlatformEndpointRouteBuilder UseLti13PlatformCore(this IEndpointRouteBuilder endpointRouteBuilder, Action<Lti13PlatformCoreEndpointsConfig>? configure = null)
        {
            var routeBuilder = new Lti13PlatformEndpointRouteBuilder(endpointRouteBuilder);

            var config = new Lti13PlatformCoreEndpointsConfig();
            configure?.Invoke(config);

            if (routeBuilder is IApplicationBuilder appBuilder)
            {
                appBuilder.Use((context, next) =>
                {
                    if (context.Request.Path == config.AuthorizationUrl && new HttpMethod(context.Request.Method) == HttpMethod.Get)
                    {
                        context.Request.Form = new FormCollection([]);
                    }

                    return next(context);
                });
            }

            routeBuilder.MapGet(config.JwksUrl,
                async (IDataService dataService) =>
                {
                    var keys = await dataService.GetPublicKeysAsync();
                    var keySet = new JsonWebKeySet();

                    foreach (var key in keys)
                    {
                        var jwk = JsonWebKeyConverter.ConvertFromSecurityKey(key);
                        jwk.Use = JsonWebKeyUseNames.Sig;
                        jwk.Alg = SecurityAlgorithms.RsaSha256;
                        keySet.Keys.Add(jwk);
                    }

                    return Results.Json(keySet, JSON_OPTIONS);
                });

            routeBuilder.Map(config.AuthorizationUrl,
                async ([AsParameters] AuthenticationRequest queryString, [FromForm] AuthenticationRequest form, IServiceProvider serviceProvider, LinkGenerator linkGenerator, HttpContext httpContext, IOptionsMonitor<Lti13PlatformConfig> config, IDataService dataService, IPlatformService platformService) =>
                {
                    const string OPENID = "openid";
                    const string ID_TOKEN = "id_token";
                    const string FORM_POST = "form_post";
                    const string NONE = "none";
                    const string INVALID_SCOPE = "invalid_scope";
                    const string INVALID_REQUEST = "invalid_request";
                    const string INVALID_CLIENT = "invalid_client";
                    const string INVALID_GRANT = "invalid_grant";
                    const string UNAUTHORIZED_CLIENT = "unauthorized_client";
                    const string AUTH_SPEC_URI = "https://www.imsglobal.org/spec/security/v1p0/#step-2-authentication-request";
                    const string LTI_SPEC_URI = "https://www.imsglobal.org/spec/lti/v1p3/#lti_message_hint-login-parameter";
                    const string SCOPE_REQUIRED = "scope must be 'openid'.";
                    const string RESPONSE_TYPE_REQUIRED = "response_type must be 'id_token'.";
                    const string RESPONSE_MODE_REQUIRED = "response_mode must be 'form_post'.";
                    const string PROMPT_REQUIRED = "prompt must be 'none'.";
                    const string NONCE_REQUIRED = "nonce is required.";
                    const string CLIENT_ID_REQUIRED = "client_id is required.";
                    const string UNKNOWN_CLIENT_ID = "client_id is unknown";
                    const string UNKNOWN_REDIRECT_URI = "redirect_uri is unknown";
                    const string LTI_MESSAGE_HINT_INVALID = "lti_message_hint is invalid";
                    const string LOGIN_HINT_REQUIRED = "login_hint is required";
                    const string USER_CLIENT_MISMATCH = "client is not authorized for user";
                    const string DEPLOYMENT_CLIENT_MISMATCH = "deployment is not for client";

                    var request = form ?? queryString;

                    /* https://datatracker.ietf.org/doc/html/rfc6749#section-5.2 */
                    /* https://www.imsglobal.org/spec/security/v1p0/#step-2-authentication-request */

                    if (request.Scope != OPENID)
                    {
                        return Results.BadRequest(new
                        {
                            Error = INVALID_SCOPE,
                            Error_Description = SCOPE_REQUIRED,
                            Error_Uri = AUTH_SPEC_URI
                        });
                    }

                    if (request.Response_Type != ID_TOKEN)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = RESPONSE_TYPE_REQUIRED, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (request.Response_Mode != FORM_POST)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = RESPONSE_MODE_REQUIRED, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (request.Prompt != NONE)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = PROMPT_REQUIRED, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Nonce))
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = NONCE_REQUIRED, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Login_Hint))
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = LOGIN_HINT_REQUIRED, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Client_Id))
                    {
                        return Results.BadRequest(new { Error = INVALID_CLIENT, Error_Description = CLIENT_ID_REQUIRED, Error_Uri = AUTH_SPEC_URI });
                    }

                    var tool = await dataService.GetToolByClientIdAsync(request.Client_Id);

                    if (tool == null)
                    {
                        return Results.BadRequest(new { Error = INVALID_CLIENT, Error_Description = UNKNOWN_CLIENT_ID, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (!tool.RedirectUrls.Contains(request.Redirect_Uri))
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = UNKNOWN_REDIRECT_URI, Error_Uri = AUTH_SPEC_URI });
                    }

                    var userId = request.Login_Hint;
                    var user = await dataService.GetUserAsync(userId);

                    if (user == null)
                    {
                        return Results.BadRequest(new { Error = UNAUTHORIZED_CLIENT, Error_Description = USER_CLIENT_MISMATCH });
                    }

                    if (string.IsNullOrWhiteSpace(request.Lti_Message_Hint) ||
                        request.Lti_Message_Hint.Split('|', 5) is not [var messageTypeString, var deploymentId, var contextId, var resourceLinkId, var messageHintString])
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = LTI_MESSAGE_HINT_INVALID, Error_Uri = LTI_SPEC_URI });
                    }

                    var deployment = await dataService.GetDeploymentAsync(deploymentId);

                    if (deployment?.ToolId != tool.ClientId)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = DEPLOYMENT_CLIENT_MISMATCH, Error_Uri = AUTH_SPEC_URI });
                    }

                    var context = string.IsNullOrWhiteSpace(contextId) ? null : await dataService.GetContextAsync(contextId);

                    var resourceLink = string.IsNullOrWhiteSpace(resourceLinkId) ? null : await dataService.GetContentItemAsync<LtiResourceLinkContentItem>(resourceLinkId);

                    var ltiMessage = serviceProvider.GetKeyedService<LtiMessage>(messageTypeString) ?? throw new NotImplementedException($"LTI Message Type {messageTypeString} has not been registered.");

                    ltiMessage.Audience = tool.ClientId;
                    ltiMessage.IssuedDate = DateTime.UtcNow;
                    ltiMessage.Issuer = config.CurrentValue.Issuer;
                    ltiMessage.Nonce = request.Nonce!;
                    ltiMessage.ExpirationDate = DateTime.UtcNow.AddSeconds(config.CurrentValue.IdTokenExpirationSeconds);

                    ltiMessage.Subject = user.Id;

                    ltiMessage.Address = user.Address == null || !tool.UserPermissions.Address ? null : new AddressClaim
                    {
                        Country = tool.UserPermissions.AddressCountry ? user.Address.Country : null,
                        Formatted = tool.UserPermissions.AddressFormatted ? user.Address.Formatted : null,
                        Locality = tool.UserPermissions.AddressLocality ? user.Address.Locality : null,
                        PostalCode = tool.UserPermissions.AddressPostalCode ? user.Address.PostalCode : null,
                        Region = tool.UserPermissions.AddressRegion ? user.Address.Region : null,
                        StreetAddress = tool.UserPermissions.AddressStreetAddress ? user.Address.StreetAddress : null
                    };

                    ltiMessage.Birthdate = tool.UserPermissions.Birthdate ? user.Birthdate : null;
                    ltiMessage.Email = tool.UserPermissions.Email ? user.Email : null;
                    ltiMessage.EmailVerified = tool.UserPermissions.EmailVerified ? user.EmailVerified : null;
                    ltiMessage.FamilyName = tool.UserPermissions.FamilyName ? user.FamilyName : null;
                    ltiMessage.Gender = tool.UserPermissions.Gender ? user.Gender : null;
                    ltiMessage.GivenName = tool.UserPermissions.GivenName ? user.GivenName : null;
                    ltiMessage.Locale = tool.UserPermissions.Locale ? user.Locale : null;
                    ltiMessage.MiddleName = tool.UserPermissions.MiddleName ? user.MiddleName : null;
                    ltiMessage.Name = tool.UserPermissions.Name ? user.Name : null;
                    ltiMessage.Nickname = tool.UserPermissions.Nickname ? user.Nickname : null;
                    ltiMessage.PhoneNumber = tool.UserPermissions.PhoneNumber ? user.PhoneNumber : null;
                    ltiMessage.PhoneNumberVerified = tool.UserPermissions.PhoneNumberVerified ? user.PhoneNumberVerified : null;
                    ltiMessage.Picture = tool.UserPermissions.Picture ? user.Picture : null;
                    ltiMessage.PreferredUsername = tool.UserPermissions.PreferredUsername ? user.PreferredUsername : null;
                    ltiMessage.Profile = tool.UserPermissions.Profile ? user.Profile : null;
                    ltiMessage.UpdatedAt = tool.UserPermissions.UpdatedAt ? user.UpdatedAt : null;
                    ltiMessage.Website = tool.UserPermissions.Website ? user.Website : null;
                    ltiMessage.TimeZone = tool.UserPermissions.TimeZone ? user.TimeZone : null;

                    ltiMessage.MessageType = messageTypeString;

                    var scope = new Lti13MessageScope
                    {
                        Tool = tool,
                        User = user,
                        Deployment = deployment,
                        Context = context,
                        ResourceLink = resourceLink,
                        MessageHint = messageHintString
                    };

                    var services = serviceProvider.GetKeyedServices<Populator>(messageTypeString);
                    foreach (var service in services)
                    {
                        await service.Populate(ltiMessage, scope);
                    }

                    var privateKey = await dataService.GetPrivateKeyAsync();

                    var token = new JsonWebTokenHandler().CreateToken(
                        JsonSerializer.Serialize(ltiMessage, LTI_MESSAGE_JSON_SERIALIZER_OPTIONS),
                        new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256) { CryptoProviderFactory = CRYPTO_PROVIDER_FACTORY });

                    return Results.Content(@$"<!DOCTYPE html>
                        <html>
                        <body>
                        <form method=""post"" action=""{request.Redirect_Uri}"">
                        <input type=""hidden"" name=""id_token"" value=""{token}""/>
                        {(!string.IsNullOrWhiteSpace(request.State) ? @$"<input type=""hidden"" name=""state"" value=""{request.State}"" />" : null)}
                        </form>
                        <script type=""text/javascript"">
                        document.getElementsByTagName('form')[0].submit();
                        </script>
                        </body>
                        </html>",
                        MediaTypeNames.Text.Html);
                })
                .DisableAntiforgery();

            routeBuilder.MapPost(config.TokenUrl,
                async ([FromForm] TokenRequest request, LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, IDataService dataService, IOptionsMonitor<Lti13PlatformConfig> config) =>
                {
                    const string AUTH_SPEC_URI = "https://www.imsglobal.org/spec/security/v1p0/#using-json-web-tokens-with-oauth-2-0-client-credentials-grant";
                    const string SCOPE_SPEC_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0";
                    const string TOKEN_SPEC_URI = "https://www.imsglobal.org/spec/lti/v1p3/#token-endpoint-claim-and-services";
                    const string UNSUPPORTED_GRANT_TYPE = "unsupported_grant_type";
                    const string INVALID_GRANT = "invalid_grant";
                    const string CLIENT_CREDENTIALS = "client_credentials";
                    const string GRANT_REQUIRED = "grant_type must be 'client_credentials'";
                    const string CLIENT_ASSERTION_TYPE = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
                    const string CLIENT_ASSERTION_TYPE_REQUIRED = "client_assertion_type must be 'urn:ietf:params:oauth:client-assertion-type:jwt-bearer'";
                    const string INVALID_SCOPE = "invalid_scope";
                    const string SCOPE_REQUIRED = "scope must be a valid value";
                    const string CLIENT_ASSERTION_INVALID = "client_assertion must be a valid jwt";
                    const string INVALID_REQUEST = "invalid_request";
                    const string JTI_REUSE = "jti has already been used and is not expired";
                    const string BODY_MISSING = "request body is missing";

                    if (request == null)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = BODY_MISSING, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (request.Grant_Type != CLIENT_CREDENTIALS)
                    {
                        return Results.BadRequest(new { Error = UNSUPPORTED_GRANT_TYPE, Error_Description = GRANT_REQUIRED, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (request.Client_Assertion_Type != CLIENT_ASSERTION_TYPE)
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_TYPE_REQUIRED, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Scope))
                    {
                        return Results.BadRequest(new { Error = INVALID_SCOPE, Error_Description = SCOPE_REQUIRED, Error_Uri = SCOPE_SPEC_URI });
                    }

                    // TODO: filter out scopes that aren't supported by the tool
                    var scopes = HttpUtility.UrlDecode(request.Scope).Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                    if (!scopes.Any())
                    {
                        return Results.BadRequest(new { Error = INVALID_SCOPE, Error_Description = SCOPE_REQUIRED, Error_Uri = SCOPE_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Client_Assertion))
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = AUTH_SPEC_URI });
                    }

                    var jwt = new JsonWebToken(request.Client_Assertion);

                    if (jwt.Issuer != jwt.Subject)
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = TOKEN_SPEC_URI });
                    }

                    var tool = await dataService.GetToolByClientIdAsync(jwt.Issuer);
                    if (tool?.Jwks == null)
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = TOKEN_SPEC_URI });
                    }

                    var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(request.Client_Assertion, new TokenValidationParameters
                    {
                        IssuerSigningKeys = await tool.Jwks.GetKeysAsync(),
                        ValidAudience = config.CurrentValue.TokenAudience ?? (httpContextAccessor?.HttpContext == null ? null : linkGenerator.GetUriByName(httpContextAccessor.HttpContext, RouteNames.TOKEN)),
                        ValidIssuer = tool.ClientId.ToString()
                    });

                    if (!validatedToken.IsValid)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = validatedToken.Exception.Message, Error_Uri = AUTH_SPEC_URI });
                    }
                    else
                    {
                        var serviceToken = await dataService.GetServiceTokenRequestAsync(validatedToken.SecurityToken.Id);
                        if (serviceToken?.Expiration < DateTime.UtcNow)
                        {
                            return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = JTI_REUSE, Error_Uri = AUTH_SPEC_URI });
                        }

                        await dataService.SaveServiceTokenRequestAsync(new ServiceToken { Id = validatedToken.SecurityToken.Id, Expiration = validatedToken.SecurityToken.ValidTo });
                    }

                    var privateKey = await dataService.GetPrivateKeyAsync();

                    var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
                    {
                        Subject = validatedToken.ClaimsIdentity,
                        Issuer = config.CurrentValue.Issuer,
                        Audience = config.CurrentValue.Issuer,
                        Expires = DateTime.UtcNow.AddSeconds(config.CurrentValue.AccessTokenExpirationSeconds),
                        SigningCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256),
                        Claims = new Dictionary<string, object>
                        {
                            { ClaimTypes.Role, scopes }
                        }
                    });

                    return Results.Ok(new
                    {
                        access_token = token,
                        token_type = "bearer",
                        expires_in = config.CurrentValue.AccessTokenExpirationSeconds * 60,
                        scope = string.Join(' ', scopes)
                    });
                })
                .WithName(RouteNames.TOKEN)
                .DisableAntiforgery();

            return routeBuilder;
        }
    }

    internal static class RouteNames
    {
        public const string TOKEN = "TOKEN";
    }

    public record AuthenticationRequest(string? Scope, string? Response_Type, string? Response_Mode, string? Prompt, string? Nonce, string? State, string? Client_Id, string? Redirect_Uri, string? Login_Hint, string? Lti_Message_Hint);

    internal record TokenRequest(string Grant_Type, string Client_Assertion_Type, string Client_Assertion, string Scope);

    public class LtiServicesAuthHandler(IDataService dataService, IOptionsMonitor<Lti13PlatformConfig> config, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) :
        AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "NP.Lti13Platform.Services";

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //// TODO: Testing only
            //return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity([
            //    new Claim(ClaimTypes.Role, Lti13ServiceScopes.Score),
            //    new Claim(ClaimTypes.Role, Lti13ServiceScopes.ResultReadOnly),
            //    new Claim(ClaimTypes.Role, Lti13ServiceScopes.LineItem),
            //    new Claim(ClaimTypes.Role, Lti13ServiceScopes.LineItemReadOnly)
            //    ])), SchemeName));

            var authHeaderParts = Context.Request.Headers.Authorization.ToString().Trim().Split(' ');

            if (authHeaderParts.Length != 2 || authHeaderParts[0] != "Bearer")
            {
                return AuthenticateResult.NoResult();
            }

            var publicKeys = await dataService.GetPublicKeysAsync();

            var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(authHeaderParts[1], new TokenValidationParameters
            {
                IssuerSigningKeys = publicKeys,
                ValidAudience = config.CurrentValue.Issuer,
                ValidIssuer = config.CurrentValue.Issuer
            });

            return validatedToken.IsValid ? AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal([validatedToken.ClaimsIdentity]), SchemeName)) : AuthenticateResult.NoResult();
        }
    }

    public enum ActivityProgress
    {
        Initialized,
        Started,
        InProgress,
        Submitted,
        Completed
    }

    public enum GradingProgress
    {
        FullyGraded,
        Pending,
        PendingManual,
        Failed,
        NotReady
    }
}
