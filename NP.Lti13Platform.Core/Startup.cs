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

namespace NP.Lti13Platform.Core;

/// <summary>
/// Provides extension methods for configuring and using LTI 1.3 platform core services.
/// </summary>
public static class Startup
{
    const string OpenAPI_Tag = "LTI 1.3 Core";
    private static readonly CryptoProviderFactory CRYPTO_PROVIDER_FACTORY = new() { CacheSignatureProviders = false };
    private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
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
    private static readonly JsonSerializerOptions LTI_MESSAGE_JSON_SERIALIZER_OPTIONS = new(JSON_SERIALIZER_OPTIONS)
    {
        TypeInfoResolver = new LtiMessageTypeResolver(),
    };

    /// <summary>
    /// Adds the LTI 1.3 platform core services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>A <see cref="Lti13PlatformBuilder"/> that can be used to further configure the LTI 1.3 platform services.</returns>
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
            .Validate(x => x.Issuer.Scheme == Uri.UriSchemeHttps, "Lti13Platform:Token:Issuer is required when using default ILti13TokenConfigService.");
        builder.Services.TryAddSingleton<ILti13TokenConfigService, DefaultTokenConfigService>();

        return builder;
    }

    /// <summary>
    /// Configures the LTI 1.3 platform to use a custom <see cref="ILti13CoreDataService"/> implementation.
    /// </summary>
    /// <typeparam name="T">The type of the custom data service.</typeparam>
    /// <param name="builder">The <see cref="Lti13PlatformBuilder"/>.</param>
    /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> of the data service.</param>
    /// <returns>The <see cref="Lti13PlatformBuilder"/>.</returns>
    public static Lti13PlatformBuilder WithLti13CoreDataService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13CoreDataService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13CoreDataService), typeof(T), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Configures the LTI 1.3 platform to use a custom <see cref="ILti13PlatformService"/> implementation.
    /// </summary>
    /// <typeparam name="T">The type of the custom platform service.</typeparam>
    /// <param name="builder">The <see cref="Lti13PlatformBuilder"/>.</param>
    /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> of the platform service.</param>
    /// <returns>The <see cref="Lti13PlatformBuilder"/>.</returns>
    public static Lti13PlatformBuilder WithLti13PlatformService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13PlatformService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13PlatformService), typeof(T), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Configures the LTI 1.3 platform to use a custom <see cref="ILti13TokenConfigService"/> implementation.
    /// </summary>
    /// <typeparam name="T">The type of the custom token config service.</typeparam>
    /// <param name="builder">The <see cref="Lti13PlatformBuilder"/>.</param>
    /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> of the token config service.</param>
    /// <returns>The <see cref="Lti13PlatformBuilder"/>.</returns>
    public static Lti13PlatformBuilder WithLti13TokenConfigService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13TokenConfigService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13TokenConfigService), typeof(T), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Adds the LTI 1.3 platform core endpoints to the <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="endpointRouteBuilder">The <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="configure">A delegate to configure the <see cref="Lti13PlatformCoreEndpointsConfig"/>.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/>.</returns>
    public static IEndpointRouteBuilder UseLti13PlatformCore(this IEndpointRouteBuilder endpointRouteBuilder, Func<Lti13PlatformCoreEndpointsConfig, Lti13PlatformCoreEndpointsConfig>? configure = default)
    {
        Lti13PlatformBuilder.CreateTypes();

        Lti13PlatformCoreEndpointsConfig config = new();
        config = configure?.Invoke(config) ?? config;

        if (endpointRouteBuilder is IApplicationBuilder appBuilder)
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

        endpointRouteBuilder.MapGet(config.JwksUrl,
            async (ILti13CoreDataService dataService, string clientId, CancellationToken cancellationToken) =>
            {
                var keySet = new JsonWebKeySet();

                var tool = await dataService.GetToolAsync(clientId, cancellationToken);

                if (tool != null)
                {
                    var keys = await dataService.GetPublicKeysAsync(tool.Id, cancellationToken);

                    foreach (var key in keys)
                    {
                        var jwk = JsonWebKeyConverter.ConvertFromSecurityKey(key);
                        jwk.Use = JsonWebKeyUseNames.Sig;
                        jwk.Alg = SecurityAlgorithms.RsaSha256;
                        keySet.Keys.Add(jwk);
                    }
                }

                return Results.Json(keySet, JSON_SERIALIZER_OPTIONS);
            })
            .Produces<JsonWebKeySet>(contentType: MediaTypeNames.Application.Json)
            .WithGroupName(OpenApi.GroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Gets the public keys used for JWT signing verification.")
            .WithDescription("Gets the public keys used for JWT signing verification.");

        endpointRouteBuilder.MapGet(config.AuthorizationUrl,
            async ([AsParameters] AuthenticationRequest queryString, IServiceProvider serviceProvider, ILti13TokenConfigService tokenService, ILti13CoreDataService dataService, IUrlServiceHelper urlServiceHelper, CancellationToken cancellationToken) =>
            {
                return await HandleAuthorization(queryString, serviceProvider, tokenService, dataService, urlServiceHelper, cancellationToken);
            })
            .ConfigureAuthorizationEndpoint();

        endpointRouteBuilder.MapPost(config.AuthorizationUrl,
            async ([FromForm] AuthenticationRequest form, IServiceProvider serviceProvider, ILti13TokenConfigService tokenService, ILti13CoreDataService dataService, IUrlServiceHelper urlServiceHelper, CancellationToken cancellationToken) =>
            {
                return await HandleAuthorization(form, serviceProvider, tokenService, dataService, urlServiceHelper, cancellationToken);
            })
            .ConfigureAuthorizationEndpoint();

        endpointRouteBuilder.MapPost(config.TokenUrl,
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
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "request body is missing", Error_Uri = AUTH_SPEC_URI });
                }

                if (request.Grant_Type != "client_credentials")
                {
                    return Results.BadRequest(new LtiBadRequest { Error = "unsupported_grant_type", Error_Description = "grant_type must be 'client_credentials'", Error_Uri = AUTH_SPEC_URI });
                }

                if (request.Client_Assertion_Type != "urn:ietf:params:oauth:client-assertion-type:jwt-bearer")
                {
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_GRANT, Error_Description = "client_assertion_type must be 'urn:ietf:params:oauth:client-assertion-type:jwt-bearer'", Error_Uri = AUTH_SPEC_URI });
                }

                if (string.IsNullOrWhiteSpace(request.Scope))
                {
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_SCOPE, Error_Description = SCOPE_REQUIRED, Error_Uri = SCOPE_SPEC_URI });
                }

                if (string.IsNullOrWhiteSpace(request.Client_Assertion))
                {
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = AUTH_SPEC_URI });
                }

                var jwt = new JsonWebToken(request.Client_Assertion);

                if (jwt.Issuer != jwt.Subject)
                {
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = TOKEN_SPEC_URI });
                }

                var tool = await dataService.GetToolAsync(jwt.Issuer, cancellationToken);
                if (tool?.Jwks == null)
                {
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = TOKEN_SPEC_URI });
                }

                var scopes = HttpUtility.UrlDecode(request.Scope)
                    .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Intersect(tool.ServiceScopes)
                    .ToList();

                if (scopes.Count == 0)
                {
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_SCOPE, Error_Description = SCOPE_REQUIRED, Error_Uri = SCOPE_SPEC_URI });
                }

                var tokenConfig = await tokenService.GetTokenConfigAsync(tool.ClientId, cancellationToken);

                var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(request.Client_Assertion, new TokenValidationParameters
                {
                    IssuerSigningKeys = await tool.Jwks.GetKeysAsync(cancellationToken),
                    ValidAudience = tokenConfig.TokenAudience ?? linkGenerator.GetUriByName(httpContext, RouteNames.TOKEN),
                    ValidIssuer = tool.ClientId
                });

                if (!validatedToken.IsValid)
                {
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = validatedToken.Exception.Message, Error_Uri = AUTH_SPEC_URI });
                }
                else
                {
                    var serviceToken = await dataService.GetServiceTokenAsync(tool.Id, validatedToken.SecurityToken.Id, cancellationToken);
                    if (serviceToken?.Expiration > DateTime.UtcNow)
                    {
                        return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "jti has already been used and is not expired", Error_Uri = AUTH_SPEC_URI });
                    }

                    await dataService.SaveServiceTokenAsync(new ServiceToken { Id = validatedToken.SecurityToken.Id, ToolId = tool.Id, Expiration = validatedToken.SecurityToken.ValidTo }, cancellationToken);
                }

                var privateKey = await dataService.GetPrivateKeyAsync(tool.ClientId, cancellationToken);

                var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
                {
                    Subject = validatedToken.ClaimsIdentity,
                    Issuer = tokenConfig.Issuer.OriginalString,
                    Audience = tokenConfig.Issuer.OriginalString,
                    Expires = DateTime.UtcNow.AddSeconds(tokenConfig.AccessTokenExpirationSeconds),
                    SigningCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256),
                    Claims = new Dictionary<string, object>
                    {
                        { ClaimTypes.Role, scopes }
                    }
                });

                return Results.Json(new TokenResponse
                {
                    AccessToken = token,
                    TokenType = "bearer",
                    ExpiresIn = tokenConfig.AccessTokenExpirationSeconds,
                    Scope = string.Join(' ', scopes)
                }, JSON_SERIALIZER_OPTIONS);
            })
            .WithName(RouteNames.TOKEN)
            .DisableAntiforgery()
            .Produces<LtiBadRequest>(StatusCodes.Status400BadRequest)
            .Produces<TokenResponse>()
            .WithGroupName(OpenApi.GroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Gets a token to be used with platform services.")
            .WithDescription("The tool will request from this endpoint a token that will be used to authorize calls into other LTI 1.3 services.");

        return endpointRouteBuilder;
    }

    private static async Task<IResult> HandleAuthorization(AuthenticationRequest request, IServiceProvider serviceProvider, ILti13TokenConfigService tokenService, ILti13CoreDataService dataService, IUrlServiceHelper urlServiceHelper, CancellationToken cancellationToken)
    {
        const string INVALID_REQUEST = "invalid_request";
        const string INVALID_CLIENT = "invalid_client";
        const string UNAUTHORIZED_CLIENT = "unauthorized_client";
        const string AUTH_SPEC_URI = "https://www.imsglobal.org/spec/security/v1p0/#step-2-authentication-request";
        const string USER_CLIENT_MISMATCH = "client is not authorized for user";

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
            return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "response_type must be 'id_token'.", Error_Uri = AUTH_SPEC_URI });
        }

        if (request.Response_Mode != "form_post")
        {
            return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "response_mode must be 'form_post'.", Error_Uri = AUTH_SPEC_URI });
        }

        if (request.Prompt != "none")
        {
            return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "prompt must be 'none'.", Error_Uri = AUTH_SPEC_URI });
        }

        if (string.IsNullOrWhiteSpace(request.Nonce))
        {
            return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "nonce is required.", Error_Uri = AUTH_SPEC_URI });
        }

        if (string.IsNullOrWhiteSpace(request.Login_Hint))
        {
            return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "login_hint is required", Error_Uri = AUTH_SPEC_URI });
        }

        if (string.IsNullOrWhiteSpace(request.Client_Id))
        {
            return Results.BadRequest(new LtiBadRequest { Error = INVALID_CLIENT, Error_Description = "client_id is required.", Error_Uri = AUTH_SPEC_URI });
        }

        var tool = await dataService.GetToolAsync(request.Client_Id, cancellationToken);

        if (tool == null)
        {
            return Results.BadRequest(new LtiBadRequest { Error = INVALID_CLIENT, Error_Description = "client_id is unknown", Error_Uri = AUTH_SPEC_URI });
        }

        if (!tool.RedirectUrls.Contains(request.Redirect_Uri))
        {
            return Results.BadRequest(new LtiBadRequest { Error = "invalid_grant", Error_Description = "redirect_uri is unknown", Error_Uri = AUTH_SPEC_URI });
        }

        if (string.IsNullOrWhiteSpace(request.Lti_Message_Hint))
        {
            return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "lti_message_hint is invalid", Error_Uri = "https://www.imsglobal.org/spec/lti/v1p3/#lti_message_hint-login-parameter" });
        }

        var (messageTypeString, deploymentId, contextId, resourceLinkId, messageHintString) = await urlServiceHelper.ParseLtiMessageHintAsync(request.Lti_Message_Hint, cancellationToken);

        var deployment = await dataService.GetDeploymentAsync(deploymentId, cancellationToken);
        if (deployment?.ToolId != tool.Id)
        {
            return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "deployment is not for client", Error_Uri = AUTH_SPEC_URI });
        }

        var (userId, actualUserId, isAnonymous) = await urlServiceHelper.ParseLoginHintAsync(request.Login_Hint, cancellationToken);

        var user = await dataService.GetUserAsync(userId, cancellationToken);
        if (user == null)
        {
            return Results.BadRequest(new LtiBadRequest { Error = UNAUTHORIZED_CLIENT, Error_Description = USER_CLIENT_MISMATCH, Error_Uri = string.Empty });
        }

        User? actualUser = null; ;
        if (!string.IsNullOrWhiteSpace(actualUserId))
        {
            actualUser = await dataService.GetUserAsync(actualUserId, cancellationToken);

            if (actualUser == null)
            {
                return Results.BadRequest(new LtiBadRequest { Error = UNAUTHORIZED_CLIENT, Error_Description = USER_CLIENT_MISMATCH, Error_Uri = string.Empty });
            }
        }

        var context = string.IsNullOrWhiteSpace(contextId) ? null : await dataService.GetContextAsync(contextId, cancellationToken);

        var resourceLink = string.IsNullOrWhiteSpace(resourceLinkId) ? null : await dataService.GetResourceLinkAsync(resourceLinkId, cancellationToken);

        var tokenConfig = await tokenService.GetTokenConfigAsync(tool.ClientId, cancellationToken);

        var ltiMessage = serviceProvider.GetKeyedService<LtiMessage>(messageTypeString) ?? throw new NotImplementedException($"LTI Message Type {messageTypeString} has not been registered.");

        ltiMessage.MessageType = messageTypeString;

        ltiMessage.Audience = tool.ClientId;
        ltiMessage.IssuedDate = DateTime.UtcNow;
        ltiMessage.Issuer = tokenConfig.Issuer.OriginalString;
        ltiMessage.Nonce = request.Nonce!;
        ltiMessage.ExpirationDate = DateTime.UtcNow.AddSeconds(tokenConfig.MessageTokenExpirationSeconds);

        if (!isAnonymous)
        {
            var userPermissions = await dataService.GetUserPermissionsAsync(deployment.Id, contextId, user.Id, cancellationToken);

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
            ltiMessage.Picture = userPermissions.Picture ? user.Picture?.OriginalString : null;
            ltiMessage.PreferredUsername = userPermissions.PreferredUsername ? user.PreferredUsername : null;
            ltiMessage.Profile = userPermissions.Profile ? user.Profile?.OriginalString : null;
            ltiMessage.UpdatedAt = userPermissions.UpdatedAt ? user.UpdatedAt : null;
            ltiMessage.Website = userPermissions.Website ? user.Website?.OriginalString : null;
            ltiMessage.TimeZone = userPermissions.TimeZone ? user.TimeZone : null;
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

        var privateKey = await dataService.GetPrivateKeyAsync(tool.ClientId, cancellationToken);

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
    }

    private static RouteHandlerBuilder ConfigureAuthorizationEndpoint(this RouteHandlerBuilder routeHandlerBuilder)
    {
        return routeHandlerBuilder
            .DisableAntiforgery()
            .Produces<LtiBadRequest>(StatusCodes.Status400BadRequest)
            .Produces<string>(contentType: MediaTypeNames.Text.Html)
            .WithGroupName(OpenApi.GroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Callback that handles the authorization request from the tool")
            .WithDescription("After the tool receives the initial request, it will call back to this endpoint for authorization and to get the message it should handle. This endpoint will verify everything and post back to the tool with the correct message that was initially requested. Can be called as a get with query parameters or a post with a form.");
    }
}

internal record AuthenticationRequest
{
    [FromQuery(Name = "scope")]
    public string? Scope { get; set; }
    [JsonPropertyName("response_type")]
    [FromQuery(Name = "response_type")]
    public string? Response_Type { get; set; }
    [JsonPropertyName("response_mode")]
    [FromQuery(Name = "response_mode")]
    public string? Response_Mode { get; set; }
    [FromQuery(Name = "prompt")]
    public string? Prompt { get; set; }
    [FromQuery(Name = "nonce")]
    public string? Nonce { get; set; }
    [FromQuery(Name = "state")]
    public string? State { get; set; }
    [JsonPropertyName("client_id")]
    [FromQuery(Name = "client_id")]
    public string? Client_Id { get; set; }
    [JsonPropertyName("redirect_uri")]
    [FromQuery(Name = "redirect_uri")]
    public Uri? Redirect_Uri { get; set; }
    [JsonPropertyName("login_hint")]
    [FromQuery(Name = "login_hint")]
    public string? Login_Hint { get; set; }
    [JsonPropertyName("lti_message_hint")]
    [FromQuery(Name = "lti_message_hint")]
    public string? Lti_Message_Hint { get; set; }
}

internal record TokenRequest(string Grant_Type, string Client_Assertion_Type, string Client_Assertion, string Scope);

internal record TokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }
    [JsonPropertyName("token_type")]
    public required string TokenType { get; set; }
    [JsonPropertyName("expires_in")]
    public required int ExpiresIn { get; set; }
    public required string Scope { get; set; }
}

/// <summary>
/// Represents launch presentation override settings.
/// </summary>
public record LaunchPresentationOverride
{
    /// <summary>
    /// Gets or sets the document target. See <see cref="Lti13PresentationTargetDocuments"/> for possible values.
    /// </summary>
    public string? DocumentTarget { get; set; }
    /// <summary>
    /// Gets or sets the height of the presentation target.
    /// </summary>
    public double? Height { get; set; }
    /// <summary>
    /// Gets or sets the width of the presentation target.
    /// </summary>
    public double? Width { get; set; }
    /// <summary>
    /// Gets or sets the return URL.
    /// </summary>
    public string? ReturnUrl { get; set; }
    /// <summary>
    /// Gets or sets the locale.
    /// </summary>
    public string? Locale { get; set; }
}