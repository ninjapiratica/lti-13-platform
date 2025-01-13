using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Populators;
using NP.Lti13Platform.Core.Services;
using System.Collections;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Web;

namespace NP.Lti13Platform.Core
{
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

        public static Lti13PlatformBuilder AddLti13PlatformCore(this IServiceCollection serviceCollection)
        {
            var builder = new Lti13PlatformBuilder(serviceCollection);

            builder.Services.AddTransient<IUrlServiceHelper, UrlServiceHelper>();

            builder
                .ExtendLti13Message<IResourceLinkMessage, ResourceLinkPopulator>(Lti13MessageType.LtiResourceLinkRequest)
                .ExtendLti13Message<IPlatformMessage, PlatformPopulator>(Lti13MessageType.LtiResourceLinkRequest)
                .ExtendLti13Message<IContextMessage, ContextPopulator>(Lti13MessageType.LtiResourceLinkRequest)
                .ExtendLti13Message<ICustomMessage, CustomPopulator>(Lti13MessageType.LtiResourceLinkRequest)
                .ExtendLti13Message<IRolesMessage, RolesPopulator>(Lti13MessageType.LtiResourceLinkRequest);

            builder.Services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, LtiServicesAuthHandler>(LtiServicesAuthHandler.SchemeName, null);

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddOptions<Platform>().BindConfiguration("Lti13Platform:Platform");
            builder.Services.TryAddSingleton<ILti13PlatformService, DefaultPlatformService>();

            builder.Services.AddOptions<Lti13PlatformTokenConfig>()
                .BindConfiguration("Lti13Platform:Token")
                .Validate(x => !string.IsNullOrWhiteSpace(x.Issuer), "Lti13Platform:Token:Issuer is required when using default ILti13TokenConfigService.");
            builder.Services.TryAddSingleton<ILti13TokenConfigService, DefaultTokenConfigService>();

            return builder;
        }

        public static Lti13PlatformBuilder WithLti13CoreDataService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13CoreDataService
        {
            builder.Services.Add(new ServiceDescriptor(typeof(ILti13CoreDataService), typeof(T), serviceLifetime));
            return builder;
        }

        public static Lti13PlatformBuilder WithLti13PlatformService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13PlatformService
        {
            builder.Services.Add(new ServiceDescriptor(typeof(ILti13PlatformService), typeof(T), serviceLifetime));
            return builder;
        }

        public static Lti13PlatformBuilder WithLti13TokenConfigService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13TokenConfigService
        {
            builder.Services.Add(new ServiceDescriptor(typeof(ILti13TokenConfigService), typeof(T), serviceLifetime));
            return builder;
        }

        public static IEndpointRouteBuilder UseLti13PlatformCore(this IEndpointRouteBuilder routeBuilder, Func<Lti13PlatformCoreEndpointsConfig, Lti13PlatformCoreEndpointsConfig>? configure = null)
        {
            Lti13PlatformBuilder.CreateTypes();

            Lti13PlatformCoreEndpointsConfig config = new();
            config = configure?.Invoke(config) ?? config;

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
                async (ILti13CoreDataService dataService, CancellationToken cancellationToken) =>
                {
                    var keys = await dataService.GetPublicKeysAsync(cancellationToken);
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
                async ([AsParameters] AuthenticationRequest queryString, [FromForm] AuthenticationRequest form, IServiceProvider serviceProvider, ILti13TokenConfigService tokenService, ILti13CoreDataService dataService, IUrlServiceHelper urlServiceHelper, CancellationToken cancellationToken) =>
                {
                    const string INVALID_REQUEST = "invalid_request";
                    const string INVALID_CLIENT = "invalid_client";
                    const string UNAUTHORIZED_CLIENT = "unauthorized_client";
                    const string AUTH_SPEC_URI = "https://www.imsglobal.org/spec/security/v1p0/#step-2-authentication-request";
                    const string USER_CLIENT_MISMATCH = "client is not authorized for user";

                    var request = form ?? queryString;

                    /* https://datatracker.ietf.org/doc/html/rfc6749#section-5.2 */
                    /* https://www.imsglobal.org/spec/security/v1p0/#step-2-authentication-request */

                    if (request.Scope != "openid")
                    {
                        return Results.BadRequest(new
                        {
                            Error = "invalid_scope",
                            Error_Description = "scope must be 'openid'.",
                            Error_Uri = AUTH_SPEC_URI
                        });
                    }

                    if (request.Response_Type != "id_token")
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "response_type must be 'id_token'.", Error_Uri = AUTH_SPEC_URI });
                    }

                    if (request.Response_Mode != "form_post")
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "response_mode must be 'form_post'.", Error_Uri = AUTH_SPEC_URI });
                    }

                    if (request.Prompt != "none")
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "prompt must be 'none'.", Error_Uri = AUTH_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Nonce))
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "nonce is required.", Error_Uri = AUTH_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Login_Hint))
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "login_hint is required", Error_Uri = AUTH_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Client_Id))
                    {
                        return Results.BadRequest(new { Error = INVALID_CLIENT, Error_Description = "client_id is required.", Error_Uri = AUTH_SPEC_URI });
                    }

                    var tool = await dataService.GetToolAsync(request.Client_Id, cancellationToken);

                    if (tool == null)
                    {
                        return Results.BadRequest(new { Error = INVALID_CLIENT, Error_Description = "client_id is unknown", Error_Uri = AUTH_SPEC_URI });
                    }

                    if (!tool.RedirectUrls.Contains(request.Redirect_Uri))
                    {
                        return Results.BadRequest(new { Error = "invalid_grant", Error_Description = "redirect_uri is unknown", Error_Uri = AUTH_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Lti_Message_Hint))
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "lti_message_hint is invalid", Error_Uri = "https://www.imsglobal.org/spec/lti/v1p3/#lti_message_hint-login-parameter" });
                    }

                    var (messageTypeString, deploymentId, contextId, resourceLinkId, messageHintString) = await urlServiceHelper.ParseLtiMessageHintAsync(request.Lti_Message_Hint, cancellationToken);

                    var deployment = await dataService.GetDeploymentAsync(deploymentId, cancellationToken);
                    if (deployment?.ToolId != tool.Id)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "deployment is not for client", Error_Uri = AUTH_SPEC_URI });
                    }

                    var (userId, actualUserId, isAnonymous) = await urlServiceHelper.ParseLoginHintAsync(request.Login_Hint, cancellationToken);

                    var user = await dataService.GetUserAsync(userId, cancellationToken);
                    if (user == null)
                    {
                        return Results.BadRequest(new { Error = UNAUTHORIZED_CLIENT, Error_Description = USER_CLIENT_MISMATCH });
                    }

                    User? actualUser = null; ;
                    if (!string.IsNullOrWhiteSpace(actualUserId))
                    {
                        actualUser = await dataService.GetUserAsync(actualUserId, cancellationToken);

                        if (actualUser == null)
                        {
                            return Results.BadRequest(new { Error = UNAUTHORIZED_CLIENT, Error_Description = USER_CLIENT_MISMATCH });
                        }
                    }

                    var context = string.IsNullOrWhiteSpace(contextId) ? null : await dataService.GetContextAsync(contextId, cancellationToken);

                    var resourceLink = string.IsNullOrWhiteSpace(resourceLinkId) ? null : await dataService.GetResourceLinkAsync(resourceLinkId, cancellationToken);

                    var tokenConfig = await tokenService.GetTokenConfigAsync(tool.ClientId, cancellationToken);

                    var ltiMessage = serviceProvider.GetKeyedService<LtiMessage>(messageTypeString) ?? throw new NotImplementedException($"LTI Message Type {messageTypeString} has not been registered.");

                    ltiMessage.MessageType = messageTypeString;

                    ltiMessage.Audience = tool.ClientId;
                    ltiMessage.IssuedDate = DateTime.UtcNow;
                    ltiMessage.Issuer = tokenConfig.Issuer;
                    ltiMessage.Nonce = request.Nonce!;
                    ltiMessage.ExpirationDate = DateTime.UtcNow.AddSeconds(tokenConfig.MessageTokenExpirationSeconds);

                    if (!isAnonymous)
                    {
                        var userPermissions = await dataService.GetUserPermissionsAsync(deployment.Id, user.Id, cancellationToken);

                        ltiMessage.Subject = user.Id;

                        ltiMessage.Address = user.Address == null || !userPermissions.Address ? null : new AddressClaim
                        {
                            Country = userPermissions.AddressCountry ? user.Address.Country : null,
                            Formatted = userPermissions.AddressFormatted ? user.Address.Formatted : null,
                            Locality = userPermissions.AddressLocality ? user.Address.Locality : null,
                            PostalCode = userPermissions.AddressPostalCode ? user.Address.PostalCode : null,
                            Region = userPermissions.AddressRegion ? user.Address.Region : null,
                            StreetAddress = userPermissions.AddressStreetAddress ? user.Address.StreetAddress : null
                        };

                        ltiMessage.Birthdate = userPermissions.Birthdate ? user.Birthdate : null;
                        ltiMessage.Email = userPermissions.Email ? user.Email : null;
                        ltiMessage.EmailVerified = userPermissions.EmailVerified ? user.EmailVerified : null;
                        ltiMessage.FamilyName = userPermissions.FamilyName ? user.FamilyName : null;
                        ltiMessage.Gender = userPermissions.Gender ? user.Gender : null;
                        ltiMessage.GivenName = userPermissions.GivenName ? user.GivenName : null;
                        ltiMessage.Locale = userPermissions.Locale ? user.Locale : null;
                        ltiMessage.MiddleName = userPermissions.MiddleName ? user.MiddleName : null;
                        ltiMessage.Name = userPermissions.Name ? user.Name : null;
                        ltiMessage.Nickname = userPermissions.Nickname ? user.Nickname : null;
                        ltiMessage.PhoneNumber = userPermissions.PhoneNumber ? user.PhoneNumber : null;
                        ltiMessage.PhoneNumberVerified = userPermissions.PhoneNumberVerified ? user.PhoneNumberVerified : null;
                        ltiMessage.Picture = userPermissions.Picture ? user.Picture?.ToString() : null;
                        ltiMessage.PreferredUsername = userPermissions.PreferredUsername ? user.PreferredUsername : null;
                        ltiMessage.Profile = userPermissions.Profile ? user.Profile?.ToString() : null;
                        ltiMessage.UpdatedAt = userPermissions.UpdatedAt ? user.UpdatedAt : null;
                        ltiMessage.Website = userPermissions.Website ? user.Website?.ToString() : null;
                        ltiMessage.TimeZone = userPermissions.TimeZone ? user.TimeZone?.Id : null;
                    }

                    var scope = new MessageScope(
                        new UserScope(user, actualUser, isAnonymous),
                        tool,
                        deployment,
                        context,
                        resourceLink,
                        messageHintString);

                    var services = serviceProvider.GetKeyedServices<Populator>(messageTypeString);
                    foreach (var service in services) // TODO: await in list
                    {
                        await service.PopulateAsync(ltiMessage, scope, cancellationToken);
                    }

                    var privateKey = await dataService.GetPrivateKeyAsync(cancellationToken);

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
                async ([FromForm] TokenRequest request, LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, ILti13CoreDataService dataService, ILti13TokenConfigService tokenService, CancellationToken cancellationToken) =>
                {
                    const string AUTH_SPEC_URI = "https://www.imsglobal.org/spec/security/v1p0/#using-json-web-tokens-with-oauth-2-0-client-credentials-grant";
                    const string SCOPE_SPEC_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0";
                    const string TOKEN_SPEC_URI = "https://www.imsglobal.org/spec/lti/v1p3/#token-endpoint-claim-and-services";
                    const string INVALID_GRANT = "invalid_grant";
                    const string INVALID_SCOPE = "invalid_scope";
                    const string SCOPE_REQUIRED = "scope must be a valid value";
                    const string CLIENT_ASSERTION_INVALID = "client_assertion must be a valid jwt";
                    const string INVALID_REQUEST = "invalid_request";

                    var httpContext = httpContextAccessor.HttpContext!;
                    if (request == null)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "request body is missing", Error_Uri = AUTH_SPEC_URI });
                    }

                    if (request.Grant_Type != "client_credentials")
                    {
                        return Results.BadRequest(new { Error = "unsupported_grant_type", Error_Description = "grant_type must be 'client_credentials'", Error_Uri = AUTH_SPEC_URI });
                    }

                    if (request.Client_Assertion_Type != "urn:ietf:params:oauth:client-assertion-type:jwt-bearer")
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = "client_assertion_type must be 'urn:ietf:params:oauth:client-assertion-type:jwt-bearer'", Error_Uri = AUTH_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Scope))
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

                    var tool = await dataService.GetToolAsync(jwt.Issuer, cancellationToken);
                    if (tool?.Jwks == null)
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = TOKEN_SPEC_URI });
                    }

                    var scopes = HttpUtility.UrlDecode(request.Scope)
                        .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .Intersect(tool.ServiceScopes)
                        .ToList();

                    if (scopes.Count == 0)
                    {
                        return Results.BadRequest(new { Error = INVALID_SCOPE, Error_Description = SCOPE_REQUIRED, Error_Uri = SCOPE_SPEC_URI });
                    }

                    var tokenConfig = await tokenService.GetTokenConfigAsync(tool.ClientId, cancellationToken);

                    var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(request.Client_Assertion, new TokenValidationParameters
                    {
                        IssuerSigningKeys = await tool.Jwks.GetKeysAsync(cancellationToken),
                        ValidAudience = tokenConfig.TokenAudience ?? linkGenerator.GetUriByName(httpContext, RouteNames.TOKEN),
                        ValidIssuer = tool.ClientId.ToString()
                    });

                    if (!validatedToken.IsValid)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = validatedToken.Exception.Message, Error_Uri = AUTH_SPEC_URI });
                    }
                    else
                    {
                        var serviceToken = await dataService.GetServiceTokenAsync(tool.Id, validatedToken.SecurityToken.Id, cancellationToken);
                        if (serviceToken?.Expiration > DateTime.UtcNow)
                        {
                            return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "jti has already been used and is not expired", Error_Uri = AUTH_SPEC_URI });
                        }

                        await dataService.SaveServiceTokenAsync(new ServiceToken { Id = validatedToken.SecurityToken.Id, ToolId = tool.Id, Expiration = validatedToken.SecurityToken.ValidTo }, cancellationToken);
                    }

                    var privateKey = await dataService.GetPrivateKeyAsync(cancellationToken);

                    var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
                    {
                        Subject = validatedToken.ClaimsIdentity,
                        Issuer = tokenConfig.Issuer,
                        Audience = tokenConfig.Issuer,
                        Expires = DateTime.UtcNow.AddSeconds(tokenConfig.AccessTokenExpirationSeconds),
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
                        expires_in = tokenConfig.AccessTokenExpirationSeconds,
                        scope = string.Join(' ', scopes)
                    });
                })
                .WithName(RouteNames.TOKEN)
                .DisableAntiforgery();

            return routeBuilder;
        }
    }

    internal record AuthenticationRequest(string? Scope, string? Response_Type, string? Response_Mode, string? Prompt, string? Nonce, string? State, string? Client_Id, Uri? Redirect_Uri, string? Login_Hint, string? Lti_Message_Hint);

    internal record TokenRequest(string Grant_Type, string Client_Assertion_Type, string Client_Assertion, string Scope);

    public record LaunchPresentationOverride
    {
        /// <summary>
        /// <see cref="Lti13PresentationTargetDocuments"/> has the list of possible values.
        /// </summary>
        public string? DocumentTarget { get; set; }
        public double? Height { get; set; }
        public double? Width { get; set; }
        public string? ReturnUrl { get; set; }
        public string? Locale { get; set; }
    }
}
