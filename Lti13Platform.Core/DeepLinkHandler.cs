using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace NP.Lti13Platform
{
    public class DeepLinkHandler(IDataService dataService, IOptionsMonitor<Lti13PlatformConfig> config, IServiceProvider serviceProvider)
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
                ValidAudience = config.CurrentValue.Issuer,
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

            return await serviceProvider.GetRequiredService<IDeepLinkContentHandler>().HandleAsync(serviceProvider, new DeepLinkResponse
            {
                Data = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/data")?.Value,
                Message = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/msg")?.Value,
                Log = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/log")?.Value,
                ErrorMessage = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errormsg")?.Value,
                ErrorLog = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errorlog")?.Value,
                ContentItems = validatedToken.ClaimsIdentity.FindAll("https://purl.imsglobal.org/spec/lti-dl/claim/content_items").Select(x => ContentItem.Parse(JsonDocument.Parse(x.Value).RootElement)),
            });
        }
    }
}