using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Models;
using System.Net.Mime;

namespace NP.Lti13Platform
{
    public class AuthenticationHandler(
        LinkGenerator linkGenerator,
        HttpContext httpContext,
        IOptionsMonitor<Lti13PlatformConfig> config,
        Service service,
        IDataService dataService)
    {
        private const string OPENID = "openid";
        private const string ID_TOKEN = "id_token";
        private const string FORM_POST = "form_post";
        private const string NONE = "none";
        private const string INVALID_SCOPE = "invalid_scope";
        private const string INVALID_REQUEST = "invalid_request";
        private const string INVALID_CLIENT = "invalid_client";
        private const string INVALID_GRANT = "invalid_grant";
        private const string UNAUTHORIZED_CLIENT = "unauthorized_client";
        private const string AUTH_SPEC_URI = "https://www.imsglobal.org/spec/security/v1p0/#step-2-authentication-request";
        private const string LTI_SPEC_URI = "https://www.imsglobal.org/spec/lti/v1p3/#lti_message_hint-login-parameter";
        private const string SCOPE_REQUIRED = "scope must be 'openid'.";
        private const string RESPONSE_TYPE_REQUIRED = "response_type must be 'id_token'.";
        private const string RESPONSE_MODE_REQUIRED = "response_mode must be 'form_post'.";
        private const string PROMPT_REQUIRED = "prompt must be 'none'.";
        private const string NONCE_REQUIRED = "nonce is required.";
        private const string CLIENT_ID_REQUIRED = "client_id is required.";
        private const string UNKNOWN_CLIENT_ID = "client_id is unknown";
        private const string INVALID_CLIENT_ID = "client_id is invalid";
        private const string UNKNOWN_REDIRECT_URI = "redirect_uri is unknown";
        private const string LTI_MESSAGE_HINT_INVALID = "lti_message_hint is invalid";
        private const string LOGIN_HINT_REQUIRED = "login_hint is required";
        private const string USER_CLIENT_MISMATCH = "client is not authorized for user";

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

            if (string.IsNullOrWhiteSpace(request.Login_Hint))
            {
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = LOGIN_HINT_REQUIRED, Error_Uri = AUTH_SPEC_URI });
            }

            Client? client;
            if (string.IsNullOrWhiteSpace(request.Client_Id) || !Guid.TryParse(request.Client_Id, out var clientId))
            {
                return Results.BadRequest(new { Error = INVALID_CLIENT, Error_Description = CLIENT_ID_REQUIRED, Error_Uri = AUTH_SPEC_URI });
            }
            else
            {
                client = await dataService.GetClientAsync(clientId);
                if (client == null)
                {
                    return Results.BadRequest(new { Error = INVALID_CLIENT, Error_Description = UNKNOWN_CLIENT_ID, Error_Uri = AUTH_SPEC_URI });
                }

                if (!client.RedirectUrls.Contains(request.Redirect_Uri))
                {
                    return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = UNKNOWN_REDIRECT_URI, Error_Uri = AUTH_SPEC_URI });
                }
            }

            if (string.IsNullOrWhiteSpace(request.Lti_Message_Hint) ||
                request.Lti_Message_Hint.Split('!', 3) is not [var messageTypeHint, var launchPresentationHint, var messageHint])
            {
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = UNAUTHORIZED_CLIENT, Error_Uri = LTI_SPEC_URI });
            }

            var userId = request.Login_Hint;
            var user = await dataService.GetUserAsync(client, userId);

            if (user == null)
            {
                return Results.BadRequest(new { Error = UNAUTHORIZED_CLIENT, Error_Description = USER_CLIENT_MISMATCH });
            }

            _ = Enum.TryParse(messageTypeHint, out Lti13MessageType messageType);

            (ILti13Message? ltiMessage, Context? context) = messageType switch
            {
                Lti13MessageType.LtiResourceLinkRequest => await service.ParseResourceLinkRequestHintAsync(client, messageHint),
                Lti13MessageType.LtiDeepLinkingRequest => await service.ParseDeepLinkRequestHintAsync(messageHint),
                _ => (null, null)
            };

            if (ltiMessage == null || context == null)
            {
                return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = LTI_MESSAGE_HINT_INVALID, Error_Uri = LTI_SPEC_URI });
            }

            var deployment = await dataService.GetDeploymentAsync(context.DeploymentId);
            if (deployment?.ClientId != client.Id)
            {
                return Results.BadRequest(new { Error = INVALID_CLIENT, Error_Description = INVALID_CLIENT_ID, Error_Uri = LTI_SPEC_URI });
            }

            var roles = await dataService.GetRolesAsync(userId, client, context);
            var mentoredUserIds = await dataService.GetMentoredUserIdsAsync(userId, client, context);
            var lineItems = await dataService.GetLineItemsAsync(context.Id, 0, 1, null, null, null); // todo: get the resourcelinkid from the requestmessage
            
            var ltiClaims = service.GetClaims(
                messageType,
                context.DeploymentId,
                ltiMessage,
                new Lti13RolesClaim { Roles = roles },

                // optional
                context,
                config.CurrentValue.PlatformClaim,
                new Lti13AgsEndpointClaim(linkGenerator, httpContext) { ContextId = context?.Id, LineItemId = lineItems.TotalItems == 1 ? lineItems.Items.FirstOrDefault()!.Id : null, Scope = [] }, // todo: add scopes/permissions to client
                new Lti13RoleScopeMentorClaim { UserIds = mentoredUserIds },
                service.ParseLaunchPresentationHint(launchPresentationHint),
                new Lti13CustomClaim());

            var privateKey = await dataService.GetPrivateKeyAsync();

            var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Issuer = config.CurrentValue.Issuer,
                Audience = request.Client_Id,
                Expires = DateTime.UtcNow.AddSeconds(config.CurrentValue.IdTokenExpirationSeconds),
                SigningCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256) { CryptoProviderFactory = CRYPTO_PROVIDER_FACTORY },
                Claims = user.GetClaims()
                    .Concat(ltiClaims)
                    .Append(KeyValuePair.Create("nonce", (object)request.Nonce))
                    .ToDictionary(c => c.Key, c => c.Value)
            });

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

