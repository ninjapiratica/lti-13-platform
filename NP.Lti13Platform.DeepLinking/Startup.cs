using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Populators;
using System.Text.Json;

namespace NP.Lti13Platform.DeepLinking
{
    public static class Startup
    {
        public static Lti13PlatformServiceCollection AddLti13PlatformDeepLinking(this Lti13PlatformServiceCollection services)
        {
            services.AddMessageHandler(Lti13MessageType.LtiDeepLinkingRequest)
                .ExtendLti13Message<IDeepLinkingMessage, PopulateDeepLinking>()
                .ExtendLti13Message<IPlatformMessage, PlatformPopulator>()
                .ExtendLti13Message<ILaunchPresentationMessage, LaunchPresentationPopulator>()
                .ExtendLti13Message<IContextMessage, ContextPopulator>()
                .ExtendLti13Message<ICustomMessage, CustomPopulator>()
                .ExtendLti13Message<IRolesMessage, RolesPopulator>();

            return services;
        }

        public static Lti13PlatformEndpointRouteBuilder UseLti13PlatformDeepLinking(this Lti13PlatformEndpointRouteBuilder app, Action<Lti13PlatformDeepLinkingEndpointsConfig>? configure = null)
        {
            var config = new Lti13PlatformDeepLinkingEndpointsConfig();
            configure?.Invoke(config);

            app.MapPost(config.DeepLinkingResponseUrl,
               async ([FromForm] DeepLinkResponseRequest request, string? contextId, ILogger<DeepLinkResponseRequest> logger, IOptionsMonitor<Lti13PlatformConfig> config, IDataService dataService, IDeepLinkContentHandler deepLinkContentHandler) =>
               {
                   const string DEEP_LINKING_SPEC = "https://www.imsglobal.org/spec/lti-dl/v2p0/#deep-linking-response-message";
                   const string INVALID_CLIENT = "invalid_client";
                   const string INVALID_REQUEST = "invalid_request";
                   const string JWT_REQUIRED = "JWT is required";
                   const string DEPLOYMENT_ID_REQUIRED = "deployment_id is required";
                   const string CLIENT_ID_REQUIRED = "client_id is required";
                   const string DEPLOYMENT_ID_INVALID = "deployment_id is invalid";
                   const string MESSAGE_TYPE_INVALID = "message_type is invalid";
                   const string VERSION_INVALID = "version is invalid";

                   if (string.IsNullOrWhiteSpace(request.Jwt))
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = JWT_REQUIRED, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var jwt = new JsonWebToken(request.Jwt);

                   if (!jwt.TryGetClaim("https://purl.imsglobal.org/spec/lti/claim/deployment_id", out var deploymentIdClaim))
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = DEPLOYMENT_ID_REQUIRED, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var deployment = await dataService.GetDeploymentAsync(deploymentIdClaim.Value);
                   if (deployment == null)
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = DEPLOYMENT_ID_INVALID, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var tool = await dataService.GetToolAsync(deployment.ToolId);
                   if (tool?.Jwks == null)
                   {
                       return Results.NotFound(new { Error = INVALID_CLIENT, Error_Description = CLIENT_ID_REQUIRED, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(request.Jwt, new TokenValidationParameters
                   {
                       IssuerSigningKeys = await tool.Jwks.GetKeysAsync(),
                       ValidAudience = config.CurrentValue.Issuer,
                       ValidIssuer = tool.ClientId.ToString()
                   });

                   if (!validatedToken.IsValid)
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = validatedToken.Exception.Message, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/message_type", out var messageType) || (string)messageType != "LtiDeepLinkingResponse")
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = MESSAGE_TYPE_INVALID, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/version", out var version) || (string)version != "1.3.0")
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = VERSION_INVALID, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var response = new DeepLinkResponse
                   {
                       Data = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/data")?.Value,
                       Message = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/msg")?.Value,
                       Log = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/log")?.Value,
                       ErrorMessage = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errormsg")?.Value,
                       ErrorLog = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errorlog")?.Value,
                       ContentItems = validatedToken.ClaimsIdentity.FindAll("https://purl.imsglobal.org/spec/lti-dl/claim/content_items")
                           .Select((x, ix) =>
                           {
                               var type = JsonDocument.Parse(x.Value).RootElement.GetProperty("type").GetString();
                               var contentItem = (ContentItem)JsonSerializer.Deserialize(x.Value, config.CurrentValue.ContentItemTypes[(tool.ClientId, type)])!;

                               contentItem.Id = ix == 0 ? new Guid().ToString() : Guid.NewGuid().ToString();
                               contentItem.DeploymentId = deployment.Id;
                               contentItem.ContextId = contextId;

                               return contentItem;
                           })
                           .ToList()
                   };

                   if (!string.IsNullOrWhiteSpace(response.Log))
                   {
                       logger.LogInformation("Deep Link Log: {DeepLinkingLog}", response.Log);
                   }

                   if (!string.IsNullOrWhiteSpace(response.ErrorLog))
                   {
                       logger.LogError("Deep Link Error: {DeepLinkingError}", response.ErrorLog);
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
                               Id = Guid.NewGuid().ToString(),
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

                   return await deepLinkContentHandler.HandleAsync(response);
               })
               .WithName(RouteNames.DEEP_LINKING_RESPONSE)
               .DisableAntiforgery();

            return app;
        }
    }

    internal record DeepLinkResponseRequest(string? Jwt);
}
