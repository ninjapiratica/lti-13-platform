﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Populators;
using NP.Lti13Platform.DeepLinking.Models;
using NP.Lti13Platform.DeepLinking.Populators;
using System.Text.Json;

namespace NP.Lti13Platform.DeepLinking
{
    public static class Startup
    {
        public static Lti13PlatformBuilder AddLti13PlatformDeepLinking(this Lti13PlatformBuilder builder, Action<Lti13DeepLinkingConfig>? configure = null)
        {
            configure ??= (config) => { };

            builder.Services.Configure(configure);

            builder.AddMessageHandler(Lti13MessageType.LtiDeepLinkingRequest)
                .Extend<IDeepLinkingMessage, DeepLinkingPopulator>()
                .Extend<IPlatformMessage, PlatformPopulator>()
                .Extend<IContextMessage, ContextPopulator>()
                .Extend<ICustomMessage, CustomPopulator>()
                .Extend<IRolesMessage, RolesPopulator>();

            return builder;
        }

        public static Lti13PlatformEndpointRouteBuilder UseLti13PlatformDeepLinking(this Lti13PlatformEndpointRouteBuilder app, Action<Lti13PlatformDeepLinkingEndpointsConfig>? configure = null)
        {
            var config = new Lti13PlatformDeepLinkingEndpointsConfig();
            configure?.Invoke(config);

            app.MapPost(config.DeepLinkingResponseUrl,
               async ([FromForm] DeepLinkResponseRequest request, string? contextId, ILogger<DeepLinkResponseRequest> logger, IOptionsMonitor<Lti13DeepLinkingConfig> deepLinkingConfig, IOptionsMonitor<Lti13PlatformConfig> platformConfig, ICoreDataService coreDataService, IDeepLinkingDataService deepLinkingDataService, IDeepLinkContentHandler deepLinkContentHandler) =>
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
                   const string UNKNOWN = "unknown";
                   const string TYPE = "type";
                   const string VERSION = "1.3.0";
                   const string LTI_DEEP_LINKING_RESPONSE = "LtiDeepLinkingResponse";

                   if (string.IsNullOrWhiteSpace(request.Jwt))
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = JWT_REQUIRED, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var jwt = new JsonWebToken(request.Jwt);
                   var clientId = jwt.Issuer;

                   if (!jwt.TryGetClaim("https://purl.imsglobal.org/spec/lti/claim/deployment_id", out var deploymentIdClaim))
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = DEPLOYMENT_ID_REQUIRED, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var deployment = await coreDataService.GetDeploymentAsync(deploymentIdClaim.Value);
                   if (deployment == null)
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = DEPLOYMENT_ID_INVALID, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var tool = await coreDataService.GetToolAsync(clientId);
                   if (tool?.Jwks == null)
                   {
                       return Results.NotFound(new { Error = INVALID_CLIENT, Error_Description = CLIENT_ID_REQUIRED, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(request.Jwt, new TokenValidationParameters
                   {
                       IssuerSigningKeys = await tool.Jwks.GetKeysAsync(),
                       ValidAudience = platformConfig.CurrentValue.Issuer,
                       ValidIssuer = tool.ClientId.ToString()
                   });

                   if (!validatedToken.IsValid)
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = validatedToken.Exception.Message, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/message_type", out var messageType) || (string)messageType != LTI_DEEP_LINKING_RESPONSE)
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = MESSAGE_TYPE_INVALID, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/version", out var version) || (string)version != VERSION)
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
                               var type = JsonDocument.Parse(x.Value).RootElement.GetProperty(TYPE).GetString() ?? UNKNOWN;

                               var contentItem = (ContentItem)JsonSerializer.Deserialize(x.Value, deepLinkingConfig.CurrentValue.ContentItemTypes[(tool.ClientId, type)])!;

                               //contentItem.Id = ix == 0 ? new Guid().ToString() : Guid.NewGuid().ToString();

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

                   if (deepLinkingConfig.CurrentValue.AutoCreate == true)
                   {
                       var saveTasks = response.ContentItems.Select(async ci =>
                       {
                           var id = await deepLinkingDataService.SaveContentItemAsync(deployment.Id, contextId, ci);

                           if (ci is LtiResourceLinkContentItem rlci && deepLinkingConfig.CurrentValue.AcceptLineItem == true && rlci.LineItem != null && contextId != null)
                           {
                               await deepLinkingDataService.SaveLineItemAsync(new LineItem
                               {
                                   Id = Guid.NewGuid().ToString(),
                                   DeploymentId = deployment.Id,
                                   ContextId = contextId,
                                   Label = rlci.LineItem!.Label ?? rlci.Title ?? rlci.Type,
                                   ScoreMaximum = rlci.LineItem.ScoreMaximum,
                                   GradesReleased = rlci.LineItem.GradesReleased,
                                   Tag = rlci.LineItem.Tag,
                                   ResourceId = rlci.LineItem.ResourceId,
                                   ResourceLinkId = id,
                                   StartDateTime = rlci.Submission?.StartDateTime,
                                   EndDateTime = rlci.Submission?.EndDateTime
                               });
                           }
                       });

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

    public static class Lti13MessageType
    {
        public const string LtiDeepLinkingRequest = "LtiDeepLinkingRequest";
    }
}
