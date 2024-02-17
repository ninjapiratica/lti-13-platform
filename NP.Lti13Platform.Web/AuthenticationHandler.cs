using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core;
using System.Security.Cryptography;

namespace NP.Lti13Platform.Web
{
    public class AuthenticationHandler(
        Lti13PlatformWebConfig config,
        IServiceProvider serviceProvider,
        UserIdentity userIdentity)
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
        private const string LTI_MESSAGE_HINT_REQUIRED = "lti_message_hint is required";
        private const string LOGIN_HINT_REQUIRED = "login_hint is required";

        private static readonly CryptoProviderFactory CRYPTO_PROVIDER_FACTORY = new() { CacheSignatureProviders = false };

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

            //string[] loginHintParts = [];
            //if (string.IsNullOrWhiteSpace(request.Login_Hint) || (loginHintParts = request.Login_Hint.Split('|')).Length < 2)
            //{
            //    return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = LOGIN_HINT_REQUIRED, Error_Uri = AUTH_SPEC_URI });
            //}

            string[] messageHintParts = [];
            if (string.IsNullOrWhiteSpace(request.Lti_Message_Hint) || (messageHintParts = request.Lti_Message_Hint.Split('|')).Length < 2)
            {
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = LTI_MESSAGE_HINT_REQUIRED, Error_Uri = AUTH_SPEC_URI });
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
            var messageTypeName = messageHintParts[0];
            var deploymentId = messageHintParts[1];

            //switch (messageTypeName)
            //{
            //    case LtiResourceLinkRequestMessage.MessageType:
            //        {
            //            var message = serviceProvider.GetRequiredService<LtiResourceLinkRequestMessage>();
            //            await foreach (var claim in message.GetClaimsAsync())
            //            {
            //                claims = claims.Append(claim);
            //            }
            //            break;
            //        }
            //    case LtiDeepLinkingRequestMessage.MessageType:
            //        {
            //            var message = serviceProvider.GetRequiredService<LtiDeepLinkingRequestMessage>();
            //            await foreach (var claim in message.GetClaimsAsync())
            //            {
            //                claims = claims.Append(claim);
            //            }
            //            break;
            //        }
            //    default:
            //        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "Unknown message type" });
            //}

            //await foreach (var claim in userIdentity.GetClaimsAsync())
            //{
            //    claims = claims.Append(claim);
            //}

            //await foreach (var claim in rolesIdentity.GetClaimsAsync())
            //{
            //    claims = claims.Append(claim);
            //}

            //await foreach (var claim in contextIdentity.GetClaimsAsync())
            //{
            //    claims = claims.Append(claim);
            //}

            //await foreach (var claim in roleScopeMentorIdentity.GetClaimsAsync())
            //{
            //    claims = claims.Append(claim);
            //}

            //await foreach (var claim in customIdentity.GetClaimsAsync())
            //{
            //    claims = claims.Append(claim);
            //}

            //await foreach (var claim in platformIdentity.GetClaimsAsync())
            //{
            //    claims = claims.Append(claim);
            //}

            //await foreach (var claim in launchPresentationIdentity.GetClaimsAsync())
            //{
            //    claims = claims.Append(claim);
            //}

            using var rsaProvider = RSA.Create();
            rsaProvider.ImportFromPem("-----BEGIN RSA PRIVATE KEY-----\r\nMIIEowIBAAKCAQEAl019/w1CjGtqDUn8Ozz63NEfSXK0WdawD3KUnOlDh4kGoRaR\r\nVIrfg/FLEraJewdSMAvTQnLzGDQFuPyI1FYmyFEHahHs/h8LJIyiy7pd9KhUsuP3\r\nKeYvd32bHGVz/u2gySOSRAZy88evvMGrFTtaqLeRUxW5BnyZut3tbpwqViXLizZq\r\nk5XfEVgbqx7V2QV9PVJf0a4u3exp72tAbQezF0q0Kipc13VfXYTZvexcrQuSWGcU\r\nzBNFOIKoETLcFK8wVbhTfo2Q0MBCMoEJogunTsZrrdEiCrF1XPIq7EFxM+JBWG1a\r\n1eDvxWnjQTqK0VVl2LfCMoRN/rD7rfDibQc0YQIDAQABAoIBAAMljnBGg1LOTRdX\r\nqZJF02XSR5dMdmnD6Ed595NH2qqv895XzM/4T2u8EfaiqztOzKvJIyynnVysgE33\r\nmpTn8ciKvt+63bXvSVkKP7yC9L9I3PIXgaVybxxKFXbCuWXc5VIpljop9CwTxBjl\r\n4jv/zwPhRXl34zA6WSwkv3JkdxDxkgTOxyoaHkbMre0CdP9Dl4AQiKyaf36IbXfh\r\nsIBOYvX5oP5Od5Y+Ug+s0aq2hjoglxImgZDyBlGCe+I+JDUPW+OvgsttFya8c3M9\r\nxJRW7BxiMoHqe+cd9EEl1nWXHqtamTPD54rmuqoVuwNM3KaLdq/dLZI/62mKlDIF\r\nHSa/TwECgYEA4m5Ivh3HjLjU3C1lG0E3WMbtxqbB5mG8n+Yogytj7POSEz0ul2JF\r\nUm2cvFnNUdWe6OSRG73QkRntJIofQAAOWSlOEFFDM1DdLRg9kLMStEo2JoNPjDAM\r\n+TSk3R1zkJNeDzNtkZO1f+WejTVcupN3qEsuTF+g+o8rAE8P73YuQdECgYEAqw+h\r\nnISXOBD2DudllXTyn7zRxXe1m98u2ZE2pZd+uRypO7PMpfVMiscsCrJoHbBkXE6J\r\n9Wb9KQgVKHxdtuaxkOuKqk1t8VjlHQyC/VO88yXwFE+yQ9nrCTEcu2o2Eb9Ey7a6\r\nbCLApgBmpU0QWLYM1wj18GP7PiATpfzHL3f7XZECgYAVJswwxkNfx9xKfQsW0q7C\r\n4kJP7j/qr3KZVTyvlBwPhGk+1tZFWe6z1n1vssvVOylPBBryBnc3Nr7KTQTCS78L\r\nYSpjp9OpNYKTtdH6dF/o643HZzjFFbAAj4RfC2NCPCHrNZikorGvstluw29YFnJ1\r\nDCDVDZHSFhGkQ75vVhDYIQKBgFlxG+R144eaPr3+ObxS4MWq+dgRRrEQmjOCXRtq\r\nQgVSOh6QXZHs16+8goe5Tv0vDNrC6hmZVweMRVvc4zdOGkwXDHMNd035WBq/PwJs\r\nNWDBVm2YWjJmECHHPymzWEAhTTxi98iwxyBFF2aZC9IGpmINOmMONAEAzqU8rX1h\r\nc9oxAoGBAIzUwcmOWndLyL9swO8TIqRODE4NuHjwmc6SyrGMAaSZZvPnP8AOO7AE\r\nXFhNjuWFaQn/zTwkNKPzWxWyuDQp/0Sk3pGV6XteV0ShuJrOSAFSR0h9GdTMH6ZK\r\nv+8h7srC/8nHogRvbhYbA7ffAvvVP0wkJRz0jaMkVeY9cMZStJzT\r\n-----END RSA PRIVATE KEY-----\r\n");

            var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Issuer = config.Core.Issuer,
                Audience = request.Client_Id,
                Expires = DateTime.UtcNow.AddMinutes(config.IdTokenExpirationMinutes),
                SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsaProvider) { CryptoProviderFactory = CRYPTO_PROVIDER_FACTORY }, SecurityAlgorithms.RsaSha256),
                Claims = claims
            });

            return Results.Ok(token);

            //            return Results.Content(@$"<!DOCTYPE html>
            //<html>
            //<body>
            //<form method=""post"" action=""{request.Redirect_Uri}"">
            //<input type=""hidden"" name=""id_token"" value=""{token}""/>
            //{(!string.IsNullOrWhiteSpace(request.State) ? @$"<input type=""hidden"" name=""state"" value=""{request.State}"" />" : null)}
            //</form>
            //<script type=""text/javascript"">
            //document.getElementsByTagName('form')[0].submit();
            //</script>
            //</body>
            //</html>", MediaTypeNames.Text.Html);
        }
    }
}

