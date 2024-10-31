using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

            return builder;
        }

        public static Lti13PlatformBuilder WithDefaultPlatformService(this Lti13PlatformBuilder builder, Action<Platform>? configure = null)
        {
            configure ??= x => { };

            builder.Services.Configure(configure);
            builder.Services.AddTransient<IPlatformService, PlatformService>();
            return builder;
        }

        public static Lti13PlatformBuilder WithDefaultTokenService(this Lti13PlatformBuilder builder, Action<Lti13PlatformTokenConfig> configure)
        {
            builder.Services.Configure(configure);
            builder.Services.AddTransient<ITokenService, TokenService>();
            return builder;
        }

        public static IEndpointRouteBuilder UseLti13PlatformCore(this IEndpointRouteBuilder routeBuilder, Action<Lti13PlatformCoreEndpointsConfig>? configure = null)
        {
            Lti13PlatformBuilder.CreateTypes();

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
                async (ICoreDataService dataService, CancellationToken cancellationToken) =>
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
                async ([AsParameters] AuthenticationRequest queryString, [FromForm] AuthenticationRequest form, IServiceProvider serviceProvider, ITokenService tokenService, ICoreDataService dataService, IUrlServiceHelper urlServiceHelper, CancellationToken cancellationToken) =>
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

                    var tool = await dataService.GetToolAsync(request.Client_Id, cancellationToken);

                    if (tool == null)
                    {
                        return Results.BadRequest(new { Error = INVALID_CLIENT, Error_Description = UNKNOWN_CLIENT_ID, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (!tool.RedirectUrls.Contains(request.Redirect_Uri))
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = UNKNOWN_REDIRECT_URI, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Lti_Message_Hint))
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = LTI_MESSAGE_HINT_INVALID, Error_Uri = LTI_SPEC_URI });
                    }

                    var (messageTypeString, deploymentId, contextId, resourceLinkId, messageHintString) = await urlServiceHelper.ParseLtiMessageHintAsync(request.Lti_Message_Hint, cancellationToken);

                    var deployment = await dataService.GetDeploymentAsync(deploymentId, cancellationToken);
                    if (deployment?.ToolId != tool.Id)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = DEPLOYMENT_CLIENT_MISMATCH, Error_Uri = AUTH_SPEC_URI });
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
                    }

                    var scope = new MessageScope(
                        new UserScope(user, actualUser, isAnonymous),
                        tool,
                        deployment,
                        context,
                        resourceLink,
                        messageHintString);

                    var services = serviceProvider.GetKeyedServices<Populator>(messageTypeString);
                    foreach (var service in services)
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
                async ([FromForm] TokenRequest request, LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, ICoreDataService dataService, ITokenService tokenService, CancellationToken cancellationToken) =>
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

                    var httpContext = httpContextAccessor.HttpContext!;
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
                        var serviceToken = await dataService.GetServiceTokenRequestAsync(tool.Id, validatedToken.SecurityToken.Id, cancellationToken);
                        if (serviceToken?.Expiration > DateTime.UtcNow)
                        {
                            return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = JTI_REUSE, Error_Uri = AUTH_SPEC_URI });
                        }

                        await dataService.SaveServiceTokenRequestAsync(new ServiceToken { Id = validatedToken.SecurityToken.Id, ToolId = tool.Id, Expiration = validatedToken.SecurityToken.ValidTo }, cancellationToken);
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

    internal record AuthenticationRequest(string? Scope, string? Response_Type, string? Response_Mode, string? Prompt, string? Nonce, string? State, string? Client_Id, string? Redirect_Uri, string? Login_Hint, string? Lti_Message_Hint);

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
