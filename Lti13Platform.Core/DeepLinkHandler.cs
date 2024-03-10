using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace NP.Lti13Platform
{
    public class DeepLinkHandler(IDataService dataService, Lti13PlatformConfig config)
    {
        public async Task<IResult> HandleAsync(DeepLinkResponseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Jwt))
            {
                return Results.BadRequest("NO JWT FOUND");
            }

            var jwt = new JsonWebToken(request.Jwt);

            if (!jwt.TryGetClaim("https://purl.imsglobal.org/spec/lti/claim/deployment_id", out var deploymentIdClaim))
            {
                return Results.BadRequest("BAD DEPLOYMENT ID");
            }

            var deployment = await dataService.GetDeploymentAsync(deploymentIdClaim.Value);
            if (deployment == null)
            {
                return Results.BadRequest("BAD DEPLOYMENT ID");
            }

            var client = await dataService.GetClientAsync(deployment.ClientId);
            if (client?.Jwks == null)
            {
                return Results.BadRequest("BAD DEPLOYMENT ID");
            }

            var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(request.Jwt, new TokenValidationParameters
            {
                IssuerSigningKeys = await client.Jwks.GetKeysAsync(),
                ValidAudience = config.Issuer,
                ValidIssuer = client.Id
            });

            if (!validatedToken.IsValid)
            {
                return Results.BadRequest(validatedToken.Exception);
            }

            if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/message_type", out var messageType) || (string)messageType != "LtiDeepLinkingResponse")
            {
                return Results.BadRequest("BAD MESSAGE TYPE");
            }

            if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/version", out var version) || (string)version != "1.3.0")
            {
                return Results.BadRequest("BAD VERSION");
            }

            var data = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/data")?.Value;
            var message = validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti-dl/claim/msg", out var messageClaim) ? (string)messageClaim : default;
            var log = validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti-dl/claim/log", out var logClaim) ? (string)logClaim : default;
            var errorMessage = validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti-dl/claim/errormsg", out var errorMessageClaim) ? (string)errorMessageClaim : default;
            var errorLog = validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti-dl/claim/errorlog", out var errorLogClaim) ? (string)errorLogClaim : default;

            try
            {
                //var contentItems = validatedToken.Claims.TryGetValue(, out var contentItemsClaim) ? (IList<object>)contentItemsClaim : default;

                validatedToken.Claims.TryGetValue("", out var val);

                var x = ((JsonElement)val).Deserialize<Test>();
                var y = (IList<object>)val;


                //var y = validatedToken.ClaimsIdentity.FindAll("https://purl.imsglobal.org/spec/lti-dl/claim/content_items").Select(x => JsonSerializer.Deserialize<Test>(x.Value));

                //var items = contentItems.Select(x => );
                return Results.Ok(new
                {
                    jwt = request.Jwt,
                    y
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new
                {
                    jwt = request.Jwt,
                    exception = ex.Message
                });
            }
        }

        public class Test
        {
            public string type { get; set; }
            public string title { get; set; }
        }
    }
}