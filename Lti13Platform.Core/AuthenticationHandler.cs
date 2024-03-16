using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mime;
using System.Security.Cryptography;

namespace NP.Lti13Platform
{
    public class AuthenticationHandler(
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

            Lti13Client? client;
            if (string.IsNullOrWhiteSpace(request.Client_Id))
            {
                return Results.BadRequest(new { Error = INVALID_CLIENT, Error_Description = CLIENT_ID_REQUIRED, Error_Uri = AUTH_SPEC_URI });
            }
            else
            {
                client = await dataService.GetClientAsync(request.Client_Id);
                if (client == null)
                {
                    return Results.BadRequest(new { Error = INVALID_CLIENT, Error_Description = UNKNOWN_CLIENT_ID, Error_Uri = AUTH_SPEC_URI });
                }

                if (!client.RedirectUris.Contains(request.Redirect_Uri))
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

            (ILti13Message? ltiMessage, Lti13Context? context) = messageType switch
            {
                Lti13MessageType.LtiResourceLinkRequest => await service.ParseResourceLinkRequestHintAsync(messageHint),
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

            var ltiClaims = service.GetClaims(
                messageType,
                context.DeploymentId,
                ltiMessage,
                new Lti13RolesClaim { Roles = roles },

                // optional
                context,
                config.CurrentValue.PlatformClaim,
                new Lti13RoleScopeMentorClaim { UserIds = mentoredUserIds },
                service.ParseLaunchPresentationHint(launchPresentationHint),
                new Lti13CustomClaim());

            using var rsaProvider = RSA.Create();
            // Test key
            rsaProvider.ImportFromPem("-----BEGIN RSA PRIVATE KEY-----\r\nMIIEpAIBAAKCAQEArSpqDfXwj0ZYB0yQTL967oaTiOI35kaudwkF2NFRPKkxF43o\r\nqtlzdX7TuTzvhVmIW8iY9ZqDVcH9av+MfA5D3YYMPnRws2+b2DE16cN+qKqonuMt\r\naj9RERLYrC2Gz2fDB612L8TZi7KV/AFESeVt3rAGGSeXc8PLRvPz/WU0o4JGnsbq\r\naY2morgcHssHWurAWlrNHM4cYnz5ku9BM2OsT3vTKjQCW9pcEfGtBuPPOhVUK879\r\n9GOZTeIsU4Uvjv+l+FoINJwRqeaoisA5nh2MxbP2CyCiAW9b0oWFRCoJwDz6HKUG\r\nVZWsclLmLosjrKK1nxHmWyy5jxxak7YNCyhfmQIDAQABAoIBAAtgblKie35+PvRO\r\necCBEquEbexKqHou30bKHKHlBp2h+Va0fQ+/H5By7P1jh9JOp+BtKmzLDPaKZdgs\r\nwpG37N+AmgyptmnOMFf2IQui9g77af6eVZ0rBxbEZ+B6ppEOc8gXrlxvELfWhiIQ\r\nrJLfnuXy0e5pz8/MPO5ZbV3oKJrWrYP1XYQ3op3y9m55mveuuxKjjOg86Jool4Zb\r\n9jTJLQnW02PeFyIoKV5IBlWvzxSKsTqFcxW4YBcIoR3/OaCq5W8DnAf7QHQn+yaU\r\nbhMipa5iBUADd/MF8CdZKbuMaqaazyheH4s/EnbMi7s/neaZ7S313asVWMTMhCp7\r\nxyDxxbECgYEA5F1EUN+jCfiwct2E2/HY7k0jG9/ETsGxA6QHds4w76NzuKiV2f/D\r\n1f3F4/DJ/GpMCXi3Lz6j8b7qm8ypPYNRhOhCPx0F3LJLWLKw7nW5oiwM9zp1LMHC\r\na6Y7ZlAWd68fqlqegG9EEXFrCGR3nOk9Xpgwc/mNFecLsrzWNzNNWJ0CgYEAwh8X\r\nh6rJcfCM8Kf0Dbl5CCKVuFieBJ8UrM36Ky+LscVPCpcXufQ9UVCED8cI86FxjDAr\r\nRe4WCUztcq6YbNl80FiWce7w+J1JQ3GU5KQTj9kSDwtzCTTwv2vjogNExNdqEHIt\r\nsLnePMXJGIZaugWKpuxhUfsFQmCAiPs+ymeHPC0CgYEAm17tdQzDE6y0+GHI3BAu\r\n5OtsgLF9EYxs0CpQvb9JwjF2MWPaGKkQZ86yTgRsmKUFuMf98lHvHzIi0v+rAeQP\r\nmZqgP+qSK3bPFrj08jj8pN7Nr4OBZ4MosS83aMQClUl8BN6EyqNpL2j4Rox8aTCz\r\nhWGMTcuy9vzsk54xLPtlm20CgYB2IYmmK86PIf4C7ZJdT8NRqgpGttbipRRl3Ksi\r\n4Lo4IoRpQ21S4kj2VPMozsypxlNdJmsPEUYjvsa5BXsIsol8GIzlJK1L/ht5iYM8\r\naITnAwg0U5lbvvXK55MNIsQUraqD+5fGdjXB8fLgk9JeZcTss+i9hO68aBGQSqT5\r\nc2seuQKBgQDgzT9ZlxqR4JLhWsmQnu6aPt+74bxJBDvoMixFGbk3DFSQgS7Ym+Jc\r\nioRRsOZhEPkO+du8QCp86Xgga075YU0HsmHMxbFkJfUrpiwIZgcB92qk+bY/u41g\r\noSkWC32K2DuZpdLuegfjmQZo0FgDlZH6bKze0liafaioEGMGalhx9Q==\r\n-----END RSA PRIVATE KEY-----\r\n");

            var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Issuer = config.CurrentValue.Issuer,
                Audience = request.Client_Id,
                Expires = DateTime.UtcNow.AddMinutes(config.CurrentValue.IdTokenExpirationMinutes),
                SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsaProvider) { KeyId = "asdf", CryptoProviderFactory = CRYPTO_PROVIDER_FACTORY }, SecurityAlgorithms.RsaSha256),
                Claims = user.GetClaims().Concat(ltiClaims).Append(new KeyValuePair<string, object>("nonce", request.Nonce)).ToDictionary(c => c.Key, c => c.Value)
            });

            //return Results.Ok(token);

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

