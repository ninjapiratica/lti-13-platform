using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace NP.Lti13Platform.Core;

public class LtiServicesAuthHandler(ILti13CoreDataService dataService, ILti13TokenConfigService tokenService, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public static readonly string SchemeName = "NP.Lti13Platform.Services";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeaderParts = Context.Request.Headers.Authorization.ToString().Trim().Split(' ');

        if (authHeaderParts.Length != 2 || authHeaderParts[0] != "Bearer")
        {
            return AuthenticateResult.NoResult();
        }

        var publicKeys = await dataService.GetPublicKeysAsync(CancellationToken.None);

        var jwt = new JsonWebToken(authHeaderParts[1]);

        var tokenConfig = await tokenService.GetTokenConfigAsync(jwt.Subject, CancellationToken.None);

        var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(authHeaderParts[1], new TokenValidationParameters
        {
            IssuerSigningKeys = publicKeys,
            ValidAudience = tokenConfig.Issuer,
            ValidIssuer = tokenConfig.Issuer
        });

        return validatedToken.IsValid ? AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal([validatedToken.ClaimsIdentity]), SchemeName)) : AuthenticateResult.NoResult();
    }
}
