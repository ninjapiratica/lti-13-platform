using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.Core.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace NP.Lti13Platform.Core;

/// <summary>
/// Authentication handler for LTI services.
/// </summary>
/// <param name="dataService">The LTI 1.3 core data service.</param>
/// <param name="tokenService">The LTI 1.3 token config service.</param>
/// <param name="options">The authentication scheme options.</param>
/// <param name="logger">The logger factory.</param>
/// <param name="encoder">The URL encoder.</param>
public class LtiServicesAuthHandler(ILti13CoreDataService dataService, ILti13TokenConfigService tokenService, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>
    /// The name of the authentication scheme.
    /// </summary>
    public static readonly string SchemeName = "NP.Lti13Platform.Services";

    /// <summary>
    /// Handles the authentication process for incoming requests by validating a Bearer token.
    /// </summary>
    /// <remarks>This method extracts the Bearer token from the Authorization header of the HTTP request,
    /// validates it using the provided token configuration and public keys, and returns an authentication result. If
    /// the token is valid, an <see cref="AuthenticateResult.Success"/> is returned with the associated claims.
    /// Otherwise, <see cref="AuthenticateResult.NoResult"/> is returned.</remarks>
    /// <returns>An <see cref="AuthenticateResult"/> indicating the outcome of the authentication process. Returns <see
    /// cref="AuthenticateResult.Success"/> if the token is valid; otherwise, <see cref="AuthenticateResult.NoResult"/>.</returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeaderParts = Context.Request.Headers.Authorization.ToString().Trim().Split(' ');

        if (authHeaderParts.Length != 2 || authHeaderParts[0] != "Bearer")
        {
            return AuthenticateResult.NoResult();
        }

        var jwt = new JsonWebToken(authHeaderParts[1]);

        var tool = await dataService.GetToolAsync(jwt.Subject, CancellationToken.None);
        if (tool == null)
        {
            return AuthenticateResult.NoResult();
        }

        var tokenConfig = await tokenService.GetTokenConfigAsync(tool.ClientId, CancellationToken.None);
        var publicKeys = await dataService.GetPublicKeysAsync(tool.Id, CancellationToken.None);

        var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(authHeaderParts[1], new TokenValidationParameters
        {
            IssuerSigningKeys = publicKeys,
            ValidAudience = tokenConfig.Issuer.OriginalString,
            ValidIssuer = tokenConfig.Issuer.OriginalString
        });

        return validatedToken.IsValid ? AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal([validatedToken.ClaimsIdentity]), SchemeName)) : AuthenticateResult.NoResult();
    }
}
