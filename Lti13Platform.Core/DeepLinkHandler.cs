using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Models;
using System.Text.Json;

namespace NP.Lti13Platform
{
    public class DeepLinkHandler(IDataService dataService, IOptionsMonitor<Lti13PlatformConfig> config, IServiceProvider serviceProvider, ILogger<DeepLinkHandler> logger)
    {
        public async Task<IResult> HandleAsync(DeepLinkResponseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Jwt))
            {
                return Results.BadRequest("NO JWT FOUND");
            }

            var jwt = new JsonWebToken(request.Jwt);

            if (!jwt.TryGetClaim("https://purl.imsglobal.org/spec/lti/claim/deployment_id", out var deploymentIdClaim) || Guid.TryParse(deploymentIdClaim.Value, out var deploymentId))
            {
                return Results.BadRequest("BAD DEPLOYMENT ID");
            }

            var deployment = await dataService.GetDeploymentAsync(deploymentId);
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
                ValidIssuer = client.Id.ToString()
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

            var dataParts = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/data")?.Value.Split('|', 2) ?? [string.Empty, string.Empty];

            var response = new DeepLinkResponse
            {
                Data = dataParts[1],
                Message = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/msg")?.Value,
                Log = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/log")?.Value,
                ErrorMessage = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errormsg")?.Value,
                ErrorLog = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errorlog")?.Value,
                ContentItems = validatedToken.ClaimsIdentity.FindAll("https://purl.imsglobal.org/spec/lti-dl/claim/content_items")
                    .Select(x =>
                    {
                        var document = JsonDocument.Parse(x.Value);
                        var property = document.RootElement.GetProperty("type");
                        var type = property.GetRawText();
                        var contentItem = (ContentItem)JsonSerializer.Deserialize(x.Value, config.CurrentValue.ContentItemTypes[(client.Id, type)])!;

                        contentItem.Id = Guid.NewGuid();
                        contentItem.DeploymentId = deploymentId;
                        contentItem.ContextId = Guid.TryParse(dataParts[0], out var contextId) ? contextId : null;

                        return contentItem;
                    })
            };

            if (!string.IsNullOrWhiteSpace(response.Log))
            {
                logger.LogInformation(response.Log);
            }

            if (!string.IsNullOrWhiteSpace(response.ErrorLog))
            {
                logger.LogError(response.ErrorLog);
            }

            if (config.CurrentValue.DeepLink.AutoCreate == true)
            {
                await dataService.SaveContentItemsAsync(response.ContentItems);
            }

            if (config.CurrentValue.DeepLink.AcceptLineItem == true)
            {
                var saveTasks = response.ContentItems
                    .OfType<LtiResourceLinkContentItem>()
                    .Where(i => i.LineItem != null)
                    .Select(i => dataService.SaveLineItemAsync(new LineItem
                    {
                        Id = Guid.NewGuid(),
                        Label = i.LineItem!.Label ?? i.Title ?? i.Type,
                        ScoreMaximum = i.LineItem.ScoreMaximum,
                        GradesReleased = i.LineItem.GradesReleased,
                        Tag = i.LineItem.Tag,
                        ResourceId = i.LineItem.ResourceId,
                        ResourceLinkId = i.Id,
                        StartDateTime = i.Submission?.StartDateTime,
                        EndDateTime = i.Submission?.EndDateTime
                    }));

                await Task.WhenAll(saveTasks);
            }

            return await serviceProvider.GetRequiredService<IDeepLinkContentHandler>().HandleAsync(response);
        }
    }
}