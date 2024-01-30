using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography;

namespace NP.Lti13Platform.Core
{
    public class AuthenticationHandler(Lti13PlatformConfig config, IServiceProvider serviceProvider)
    {
        private const string OPENID = "openid";
        private const string ID_TOKEN = "id_token";
        private const string FORM_POST = "form_post";
        private const string NONE = "none";
        private const string INVALID_SCOPE = "invalid_scope";
        private const string INVALID_REQUEST = "invalid_request";
        private const string AUTH_SPEC_URI = "https://www.imsglobal.org/spec/security/v1p0/#step-2-authentication-request";
        private const string SCOPE_REQUIRED = "scope must be 'openid'.";
        private const string RESPONSE_TYPE_REQUIRED = "response_type must be 'id_token'.";
        private const string RESPONSE_MODE_REQUIRED = "response_mode must be 'form_post'.";
        private const string PROMPT_REQUIRED = "prompt must be 'none'.";
        private const string NONCE_REQUIRED = "nonce is required.";

        public async Task<IResult> HandleAsync(AuthenticationRequest request)
        {
            /* https://datatracker.ietf.org/doc/html/rfc6749#section-5.2 */
            /* https://www.imsglobal.org/spec/security/v1p0/#step-2-authentication-request */

            if (request.Scope != OPENID)
            {
                return Results.BadRequest(new { Error = INVALID_SCOPE, Error_Description = SCOPE_REQUIRED, Error_Uri = AUTH_SPEC_URI });
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

            // Use client_id to get the tool
            // Verify there is a tool with that client_id
            // Verify the redirect_uri is for that tool
            // If provided, Verify the lti_deployment_id is for the tool

            //request.Client_Id
            //request.Lti_Deployment_Id
            //request.Redirect_Uri

            //request.Lti_Message_Hint
            //request.Login_Hint
            var parts = request.Lti_Message_Hint.Split('|');

            switch (parts[0])
            {
                case LtiResourceLinkRequestMessage.MessageType:
                    var message = serviceProvider.GetRequiredService<LtiResourceLinkRequestMessage>();

                    // TODO: Get scopes for message

                    break;
                default:
                    return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = NONCE_REQUIRED, Error_Uri = AUTH_SPEC_URI });
            }

            using var rsaProvider = RSA.Create();
            rsaProvider.ImportFromPem("");

            var securityToken = new JwtSecurityToken(
                issuer: config.Issuer,
                audience: request.Client_Id,
                expires: DateTime.UtcNow.AddMinutes(config.IdTokenExpirationMinutes),
                signingCredentials: new SigningCredentials(new RsaSecurityKey(rsaProvider) { CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false } }, SecurityAlgorithms.RsaSha256),
                claims: [
                    new Claim("", "", JsonClaimValueTypes.Json)
                ]);
            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

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
</html>", MediaTypeNames.Text.Html);
        }
    }
}

