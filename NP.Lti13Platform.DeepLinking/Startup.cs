using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

            builder.Services.AddOptions<DeepLinkingConfig>().BindConfiguration("Lti13Platform:DeepLinking");
            builder.Services.TryAddSingleton<ILti13DeepLinkingConfigService, DefaultDeepLinkingConfigService>();
            builder.Services.TryAddSingleton<ILti13DeepLinkingHandler, DefaultDeepLinkingHandler>();

            return builder;
        }

        public static Lti13PlatformBuilder WithLti13DeepLinkingDataService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13DeepLinkingDataService
        {
            builder.Services.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingDataService), typeof(T), serviceLifetime));
            return builder;
        }

        public static Lti13PlatformBuilder WithLti13DeepLinkingConfigService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13DeepLinkingConfigService
        {
            builder.Services.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingConfigService), typeof(T), serviceLifetime));
            return builder;
        }

        public static Lti13PlatformBuilder WithLti13DeepLinkingHandler<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13DeepLinkingHandler
        {
            builder.Services.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingHandler), typeof(T), serviceLifetime));
            return builder;
        }

        public static IEndpointRouteBuilder UseLti13PlatformDeepLinking(this IEndpointRouteBuilder app, Func<DeepLinkingEndpointsConfig, DeepLinkingEndpointsConfig>? configure = null)
        {
            DeepLinkingEndpointsConfig config = new();
            config = configure?.Invoke(config) ?? config;

            _ = app.MapPost(config.DeepLinkingResponseUrl,
               async ([FromForm] DeepLinkResponseRequest request, string? contextId, ILogger<DeepLinkResponseRequest> logger, ILti13TokenConfigService tokenService, ILti13CoreDataService coreDataService, ILti13DeepLinkingDataService deepLinkingDataService, ILti13DeepLinkingConfigService deepLinkingService, ILti13DeepLinkingHandler deepLinkingHandler, CancellationToken cancellationToken) =>
               {
                   const string DEEP_LINKING_SPEC = "https://www.imsglobal.org/spec/lti-dl/v2p0/#deep-linking-response-message";
                   const string INVALID_REQUEST = "invalid_request";

                   if (string.IsNullOrWhiteSpace(request.Jwt))
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "JWT is required", Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var jwt = new JsonWebToken(request.Jwt);
                   var clientId = jwt.Issuer;

                   var tool = await coreDataService.GetToolAsync(clientId, cancellationToken);
                   if (tool?.Jwks == null)
                   {
                       return Results.NotFound(new { Error = "invalid_client", Error_Description = "client_id is required", Error_Uri = DEEP_LINKING_SPEC });
                   }

                   if (!jwt.TryGetClaim("https://purl.imsglobal.org/spec/lti/claim/deployment_id", out var deploymentIdClaim))
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "deployment_id is required", Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var deployment = await coreDataService.GetDeploymentAsync(deploymentIdClaim.Value, cancellationToken);
                   if (deployment == null || deployment.ToolId != tool.Id)
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "deployment_id is invalid", Error_Uri = DEEP_LINKING_SPEC });
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

                   if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/message_type", out var messageType) || (string)messageType != "LtiDeepLinkingResponse")
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "message_type is invalid", Error_Uri = DEEP_LINKING_SPEC });
                   }

                   if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/version", out var version) || (string)version != "1.3.0")
                   {
                       return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = "version is invalid", Error_Uri = DEEP_LINKING_SPEC });
                   }

                   var deepLinkingConfig = await deepLinkingService.GetConfigAsync(tool.ClientId, cancellationToken);

                   List<(ContentItem ContentItem, LtiResourceLinkContentItem? LtiResourceLink)> contentItems = validatedToken.ClaimsIdentity.FindAll("https://purl.imsglobal.org/spec/lti-dl/claim/content_items")
                        .Select((x, ix) =>
                        {
                            var type = JsonDocument.Parse(x.Value).RootElement.GetProperty("type").GetString() ?? "unknown";
                            var customItem = (ContentItem)JsonSerializer.Deserialize(x.Value, deepLinkingConfig.ContentItemTypes[(tool.ClientId, type)])!;

                            return (customItem, type == ContentItemType.LtiResourceLink ? JsonSerializer.Deserialize<LtiResourceLinkContentItem>(x.Value) : null);
                        })
                        .ToList();

                   var response = new DeepLinkResponse
                   {
                       Data = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/data")?.Value,
                       Message = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/msg")?.Value,
                       Log = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/log")?.Value,
                       ErrorMessage = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errormsg")?.Value,
                       ErrorLog = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errorlog")?.Value,
                       ContentItems = contentItems.Select(ci => ci.ContentItem),
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
                       var saveTasks = contentItems.Select(async ci =>
                       {
                           var id = await deepLinkingDataService.SaveContentItemAsync(deployment.Id, contextId, ci.ContentItem);

                           if (deepLinkingConfig.AcceptLineItem == true && contextId != null && ci.LtiResourceLink?.LineItem != null)
                           {
                               await deepLinkingDataService.SaveLineItemAsync(new LineItem
                               {
                                   Id = string.Empty,
                                   DeploymentId = deployment.Id,
                                   ContextId = contextId,
                                   Label = ci.LtiResourceLink.LineItem!.Label ?? ci.LtiResourceLink.Title ?? ci.LtiResourceLink.Type,
                                   ScoreMaximum = ci.LtiResourceLink.LineItem.ScoreMaximum,
                                   GradesReleased = ci.LtiResourceLink.LineItem.GradesReleased,
                                   Tag = ci.LtiResourceLink.LineItem.Tag,
                                   ResourceId = ci.LtiResourceLink.LineItem.ResourceId,
                                   ResourceLinkId = id,
                                   StartDateTime = ci.LtiResourceLink.Submission?.StartDateTime?.UtcDateTime,
                                   EndDateTime = ci.LtiResourceLink.Submission?.EndDateTime?.UtcDateTime
                               });
                           }
                       });

                       await Task.WhenAll(saveTasks);
                   }

                   return await deepLinkingHandler.HandleResponseAsync(tool.ClientId, deployment.Id, contextId, response, cancellationToken);
               })
               .WithName(RouteNames.DEEP_LINKING_RESPONSE)
               .DisableAntiforgery();

            return app;
        }
    }

    internal record DeepLinkResponseRequest(string? Jwt);
}
