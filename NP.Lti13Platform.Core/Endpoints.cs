using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.MessageHandlers;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.OpenApi;
using NP.Lti13Platform.Core.Services;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace NP.Lti13Platform.Core;

/// <summary>
/// Provides extension methods for configuring and using LTI 1.3 platform core services.
/// </summary>
public static class Endpoints
{
    const string OpenAPI_Tag = "LTI 1.3 Core";
    private static readonly CryptoProviderFactory CRYPTO_PROVIDER_FACTORY = new() { CacheSignatureProviders = false };

    /// <summary>
    /// Adds the LTI 1.3 platform core endpoints to the <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="endpointRouteBuilder">The <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="configure">A delegate to configure the <see cref="EndpointsConfig"/>.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/>.</returns>
    public static IEndpointRouteBuilder UseLti13PlatformCore(this IEndpointRouteBuilder endpointRouteBuilder, Func<EndpointsConfig, EndpointsConfig>? configure = default)
    {
        EndpointsConfig config = new();
        config = configure?.Invoke(config) ?? config;

        endpointRouteBuilder.MapGet(config.JwksUrl,
            async (ClientId clientId,
                ICoreDataService dataService,
                CancellationToken cancellationToken) =>
            {
                var keySet = new JsonWebKeySet();

                var keys = await dataService.GetPublicKeysAsync(clientId, cancellationToken);

                foreach (var key in keys)
                {
                    var jwk = JsonWebKeyConverter.ConvertFromSecurityKey(key);
                    jwk.Use = JsonWebKeyUseNames.Sig;
                    jwk.Alg = SecurityAlgorithms.RsaSha256;
                    keySet.Keys.Add(jwk);
                }

                return Results.Json(keySet, JsonSerializerMessageOptions.JSON_SERIALIZER_OPTIONS);
            })
            .Produces<JsonWebKeySet>(contentType: MediaTypeNames.Application.Json)
            .WithName(RouteNames.JWKS)
            .WithGroupName(Lti13OpenApi.GroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Gets the public keys used for JWT signing verification.")
            .WithDescription("Gets the public keys used for JWT signing verification.");

        endpointRouteBuilder.MapPost(config.TokenUrl,
            async ([FromForm] TokenRequest request,
                LinkGenerator linkGenerator,
                IHttpContextAccessor httpContextAccessor,
                ICoreDataService dataService,
                ITokenConfigService tokenService,
                CancellationToken cancellationToken) =>
            {
                const string AUTH_SPEC_URI = "https://www.imsglobal.org/spec/security/v1p0/#using-json-web-tokens-with-oauth-2-0-client-credentials-grant";
                const string SCOPE_SPEC_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0";
                const string TOKEN_SPEC_URI = "https://www.imsglobal.org/spec/lti/v1p3/#token-endpoint-claim-and-services";
                const string INVALID_GRANT = "invalid_grant";
                const string INVALID_SCOPE = "invalid_scope";
                const string SCOPE_REQUIRED = "scope must be a valid value";
                const string CLIENT_ASSERTION_INVALID = "client_assertion must be a valid jwt";
                const string INVALID_REQUEST = "invalid_request";

                if (request == null)
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_REQUEST,
                        Error_Description = "request body is missing",
                        Error_Uri = AUTH_SPEC_URI
                    });
                }

                if (request.Grant_Type != "client_credentials")
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = "unsupported_grant_type",
                        Error_Description = "grant_type must be 'client_credentials'",
                        Error_Uri = AUTH_SPEC_URI
                    });
                }

                if (request.Client_Assertion_Type != "urn:ietf:params:oauth:client-assertion-type:jwt-bearer")
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_GRANT,
                        Error_Description = "client_assertion_type must be 'urn:ietf:params:oauth:client-assertion-type:jwt-bearer'",
                        Error_Uri = AUTH_SPEC_URI
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Scope))
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_SCOPE,
                        Error_Description = SCOPE_REQUIRED,
                        Error_Uri = SCOPE_SPEC_URI
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Client_Assertion))
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_GRANT,
                        Error_Description = CLIENT_ASSERTION_INVALID,
                        Error_Uri = AUTH_SPEC_URI
                    });
                }

                var jwt = new JsonWebToken(request.Client_Assertion);
                if (jwt.Issuer != jwt.Subject)
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_GRANT,
                        Error_Description = CLIENT_ASSERTION_INVALID,
                        Error_Uri = TOKEN_SPEC_URI
                    });
                }

                var tool = await dataService.GetToolAsync(new ClientId(jwt.Issuer), cancellationToken);
                if (tool?.Jwks == null)
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_GRANT,
                        Error_Description = CLIENT_ASSERTION_INVALID,
                        Error_Uri = TOKEN_SPEC_URI
                    });
                }

                var scopes = HttpUtility.UrlDecode(request.Scope)
                    .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Intersect(tool.ServiceScopes)
                    .ToList();
                if (scopes.Count == 0)
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_SCOPE,
                        Error_Description = SCOPE_REQUIRED,
                        Error_Uri = SCOPE_SPEC_URI
                    });
                }

                var jsonWebTokenHandler = new JsonWebTokenHandler();

                var tokenConfig = await tokenService.GetTokenConfigAsync(tool.ClientId, cancellationToken);
                var validatedToken = await jsonWebTokenHandler.ValidateTokenAsync(request.Client_Assertion, new TokenValidationParameters
                {
                    IssuerSigningKeys = await tool.Jwks.GetKeysAsync(cancellationToken),
                    ValidAudience = tokenConfig.TokenAudience
                        ?? linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, RouteNames.TOKEN),
                    ValidIssuer = tool.ClientId.ToString()
                });

                if (!validatedToken.IsValid)
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_REQUEST,
                        Error_Description = validatedToken.Exception.Message,
                        Error_Uri = AUTH_SPEC_URI
                    });
                }

                var serviceTokenId = new ServiceTokenId(validatedToken.SecurityToken.Id);

                var serviceToken = await dataService.GetServiceTokenAsync(tool.ClientId, serviceTokenId, cancellationToken);
                if (serviceToken?.Expiration > DateTime.UtcNow)
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_REQUEST,
                        Error_Description = "jti has already been used and is not expired",
                        Error_Uri = AUTH_SPEC_URI
                    });
                }
                await dataService.SaveServiceTokenAsync(new ServiceToken
                {
                    Id = serviceTokenId,
                    ClientId = tool.ClientId,
                    Expiration = validatedToken.SecurityToken.ValidTo
                }, cancellationToken);

                var privateKey = await dataService.GetPrivateKeyAsync(tool.ClientId, cancellationToken);

                var token = jsonWebTokenHandler.CreateToken(new SecurityTokenDescriptor
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
                }, JsonSerializerMessageOptions.JSON_SERIALIZER_OPTIONS);
            })
            .WithName(RouteNames.TOKEN)
            .DisableAntiforgery()
            .Produces<Lti13BadRequest>(StatusCodes.Status400BadRequest)
            .Produces<TokenResponse>()
            .WithGroupName(Lti13OpenApi.GroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Gets a token to be used with platform services.")
            .WithDescription("The tool will request from this endpoint a token that will be used to authorize calls into other LTI 1.3 services.");

        endpointRouteBuilder.MapGet(config.AuthenticationUrl,
            ([AsParameters] AuthenticationRequest queryString,
            ICoreDataService dataService,
            IEnumerable<IMessageHandler> lti13MessageHandlers,
            CancellationToken cancellationToken) =>
                HandleAuthentication(queryString, dataService, lti13MessageHandlers, cancellationToken)
            )
            .ConfigureAuthenticationEndpoint(RouteNames.AUTHENTICATION_GET);

        endpointRouteBuilder.MapPost(config.AuthenticationUrl,
            ([FromForm] AuthenticationRequest form,
            ICoreDataService dataService,
            IEnumerable<IMessageHandler> lti13MessageHandlers,
            CancellationToken cancellationToken) =>
                HandleAuthentication(form, dataService, lti13MessageHandlers, cancellationToken)
            )
            .ConfigureAuthenticationEndpoint(RouteNames.AUTHENTICATION_POST);

        return endpointRouteBuilder;
    }

    private static async Task<IResult> HandleAuthentication(
        AuthenticationRequest request,
        ICoreDataService dataService,
        IEnumerable<IMessageHandler> lti13MessageHandlers,
        CancellationToken cancellationToken)
    {
        const string INVALID_REQUEST = "invalid_request";
        const string INVALID_CLIENT = "invalid_client";
        const string AUTH_SPEC_URI = "https://www.imsglobal.org/spec/security/v1p0/#step-2-authentication-request";

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
            return Results.BadRequest(new Lti13BadRequest
            {
                Error = INVALID_REQUEST,
                Error_Description = "response_type must be 'id_token'.",
                Error_Uri = AUTH_SPEC_URI
            });
        }

        if (request.Response_Mode != "form_post")
        {
            return Results.BadRequest(new Lti13BadRequest
            {
                Error = INVALID_REQUEST,
                Error_Description = "response_mode must be 'form_post'.",
                Error_Uri = AUTH_SPEC_URI
            });
        }

        if (request.Prompt != "none")
        {
            return Results.BadRequest(new Lti13BadRequest
            {
                Error = INVALID_REQUEST,
                Error_Description = "prompt must be 'none'.",
                Error_Uri = AUTH_SPEC_URI
            });
        }

        if (string.IsNullOrWhiteSpace(request.Nonce))
        {
            return Results.BadRequest(new Lti13BadRequest
            {
                Error = INVALID_REQUEST,
                Error_Description = "nonce is required.",
                Error_Uri = AUTH_SPEC_URI
            });
        }

        if (string.IsNullOrWhiteSpace(request.Login_Hint))
        {
            return Results.BadRequest(new Lti13BadRequest
            {
                Error = INVALID_REQUEST,
                Error_Description = "login_hint is required",
                Error_Uri = AUTH_SPEC_URI
            });
        }

        if (string.IsNullOrWhiteSpace(request.Client_Id))
        {
            return Results.BadRequest(new Lti13BadRequest
            {
                Error = INVALID_CLIENT,
                Error_Description = "client_id is required.",
                Error_Uri = AUTH_SPEC_URI
            });
        }

        var tool = await dataService.GetToolAsync(new ClientId(request.Client_Id), cancellationToken);
        if (tool == null)
        {
            return Results.BadRequest(new Lti13BadRequest
            {
                Error = INVALID_CLIENT,
                Error_Description = "client_id is unknown",
                Error_Uri = AUTH_SPEC_URI
            });
        }

        if (!tool.RedirectUrls.Contains(request.Redirect_Uri))
        {
            return Results.BadRequest(new Lti13BadRequest
            {
                Error = "invalid_grant",
                Error_Description = "redirect_uri is unknown",
                Error_Uri = AUTH_SPEC_URI
            });
        }

        object? lti13Message = null;
        Lti13BadRequest? failedLti13MessageResult = null;
        foreach (var lti13MessageHandler in lti13MessageHandlers)
        {
            var result = await lti13MessageHandler.HandleMessageAsync(request.Login_Hint, request.Lti_Message_Hint, tool, request.Nonce, cancellationToken);

            if (result is MessageResult.SuccessResult successResult)
            {
                lti13Message = successResult.Message;
                failedLti13MessageResult = null;
                break;
            }
            else if (result is MessageResult.ErrorResult errorResult)
            {
                failedLti13MessageResult = new Lti13BadRequest
                {
                    Error = INVALID_REQUEST,
                    Error_Description = errorResult.ErrorMessage,
                    Error_Uri = "https://www.1edtech.org/standards/lti"
                };
            }
        }

        if (failedLti13MessageResult != null)
        {
            return Results.BadRequest(failedLti13MessageResult);
        }

        if (lti13Message == null)
        {
            return Results.BadRequest(new Lti13BadRequest
            {
                Error = INVALID_REQUEST,
                Error_Description = "unsupported message type",
                Error_Uri = "https://www.1edtech.org/standards/lti"
            });
        }

        var privateKey = await dataService.GetPrivateKeyAsync(tool.ClientId, cancellationToken);

        var token = new JsonWebTokenHandler().CreateToken(
            JsonSerializer.Serialize(lti13Message, JsonSerializerMessageOptions.LTI_13_MESSAGE_JSON_SERIALIZER_OPTIONS),
            new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256) { CryptoProviderFactory = CRYPTO_PROVIDER_FACTORY });

        return Results.Content($@"
<!DOCTYPE html>
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
</html>".TrimStart(),
            MediaTypeNames.Text.Html);
    }

    private static RouteHandlerBuilder ConfigureAuthenticationEndpoint(this RouteHandlerBuilder routeHandlerBuilder, string routeName)
    {
        return routeHandlerBuilder
            .WithName(routeName)
            .DisableAntiforgery()
            .Produces<Lti13BadRequest>(StatusCodes.Status400BadRequest)
            .Produces<string>(contentType: MediaTypeNames.Text.Html)
            .WithGroupName(Lti13OpenApi.GroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Callback that handles the authentication request from the tool")
            .WithDescription("After the tool receives the initial request, it will call back to this endpoint for authentication and to get the message it should handle. This endpoint will verify everything and post back to the tool with the correct message that was initially requested. Can be called as a get with query parameters or a post with a form.");
    }
}

internal record AuthenticationRequest
{
    public string? Scope { get; set; }
    public string? Response_Type { get; set; }
    public string? Response_Mode { get; set; }
    public string? Prompt { get; set; }
    public string? Nonce { get; set; }
    public string? State { get; set; }
    public string? Client_Id { get; set; }
    public Uri? Redirect_Uri { get; set; }
    public string? Login_Hint { get; set; }
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
    /// Gets or sets the document target. See <see cref="PresentationTargetDocuments"/> for possible values.
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