using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Populators;
using NP.Lti13Platform.Core.Services;
using NP.Lti13Platform.DeepLinking.Configs;
using NP.Lti13Platform.DeepLinking.Models;
using NP.Lti13Platform.DeepLinking.Populators;
using NP.Lti13Platform.DeepLinking.Services;
using System.Text.Json;

namespace NP.Lti13Platform.DeepLinking
{
    public static class Startup
    {
        public static Lti13PlatformBuilder AddLti13PlatformDeepLinking(this Lti13PlatformBuilder builder)
        {
            builder
                .ExtendLti13Message<IDeepLinkingMessage, DeepLinkingPopulator>(Lti13MessageType.LtiDeepLinkingRequest)
                .ExtendLti13Message<IPlatformMessage, PlatformPopulator>(Lti13MessageType.LtiDeepLinkingRequest)
                .ExtendLti13Message<IContextMessage, ContextPopulator>(Lti13MessageType.LtiDeepLinkingRequest)
                .ExtendLti13Message<ICustomMessage, CustomPopulator>(Lti13MessageType.LtiDeepLinkingRequest)
                .ExtendLti13Message<IRolesMessage, RolesPopulator>(Lti13MessageType.LtiDeepLinkingRequest);

            return builder;
        }

        public static Lti13PlatformBuilder AddDefaultDeepLinkingService(this Lti13PlatformBuilder builder, Action<DeepLinkingConfig>? configure = null)
        {
            configure ??= x => { };

            builder.Services.Configure(configure);
            builder.Services.AddTransient<IDeepLinkingService, DeepLinkingService>();
            return builder;
        }

        public static IEndpointRouteBuilder UseLti13PlatformDeepLinking(this IEndpointRouteBuilder app, Action<DeepLinkingEndpointsConfig>? configure = null)
        {
            var config = new DeepLinkingEndpointsConfig();
            configure?.Invoke(config);

            app.MapPost(config.DeepLinkingResponseUrl,
               async ([FromForm] DeepLinkResponseRequest request, string? contextId, ILogger<DeepLinkResponseRequest> logger, ITokenService tokenService, ICoreDataService coreDataService, IDeepLinkingDataService deepLinkingDataService, IDeepLinkingService deepLinkingService, CancellationToken cancellationToken) =>
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
                   const string DEPLOYMENT_ID_CLAIM = "https://purl.imsglobal.org/spec/lti/claim/deployment_id";

                   if (string.IsNullOrWhiteSpace(request.Jwt))
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = JWT_REQUIRED, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var jwt = new JsonWebToken(request.Jwt);
                   var clientId = jwt.Issuer;

                   var tool = await coreDataService.GetToolAsync(clientId, cancellationToken);
                   if (tool?.Jwks == null)
                   {
                       return Results.NotFound(new { Error = INVALID_CLIENT, Error_Description = CLIENT_ID_REQUIRED, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   if (!jwt.TryGetClaim(DEPLOYMENT_ID_CLAIM, out var deploymentIdClaim))
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = DEPLOYMENT_ID_REQUIRED, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var deployment = await coreDataService.GetDeploymentAsync(deploymentIdClaim.Value, cancellationToken);
                   if (deployment == null || deployment.ToolId != tool.Id)
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = DEPLOYMENT_ID_INVALID, Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var tokenConfig = await tokenService.GetTokenConfigAsync(tool.ClientId, cancellationToken);

                   var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(request.Jwt, new TokenValidationParameters
                   {
                       IssuerSigningKeys = await tool.Jwks.GetKeysAsync(cancellationToken),
                       ValidAudience = tokenConfig.Issuer,
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

                   var deepLinkingConfig = await deepLinkingService.GetConfigAsync(tool.ClientId, cancellationToken);

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
                               return (ContentItem)JsonSerializer.Deserialize(x.Value, deepLinkingConfig.ContentItemTypes[(tool.ClientId, type)])!;
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

                   if (deepLinkingConfig.AutoCreate == true)
                   {
                       var saveTasks = response.ContentItems.Select(async ci =>
                       {
                           var id = await deepLinkingDataService.SaveContentItemAsync(deployment.Id, contextId, ci);

                           if (ci is LtiResourceLinkContentItem rlci && deepLinkingConfig.AcceptLineItem == true && rlci.LineItem != null && contextId != null)
                           {
                               await deepLinkingDataService.SaveLineItemAsync(new LineItem
                               {
                                   Id = string.Empty,
                                   DeploymentId = deployment.Id,
                                   ContextId = contextId,
                                   Label = rlci.LineItem!.Label ?? rlci.Title ?? rlci.Type,
                                   ScoreMaximum = rlci.LineItem.ScoreMaximum,
                                   GradesReleased = rlci.LineItem.GradesReleased,
                                   Tag = rlci.LineItem.Tag,
                                   ResourceId = rlci.LineItem.ResourceId,
                                   ResourceLinkId = id,
                                   StartDateTime = rlci.Submission?.StartDateTime?.UtcDateTime,
                                   EndDateTime = rlci.Submission?.EndDateTime?.UtcDateTime
                               });
                           }
                       });

                       await Task.WhenAll(saveTasks);
                   }

                   return await deepLinkingService.HandleResponseAsync(response, cancellationToken);
               })
               .WithName(RouteNames.DEEP_LINKING_RESPONSE)
               .DisableAntiforgery();

            return app;
        }
    }

    internal record DeepLinkResponseRequest(string? Jwt);
}
