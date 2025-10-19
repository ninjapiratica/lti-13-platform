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

namespace NP.Lti13Platform.DeepLinking;

/// <summary>
/// Provides extension methods to configure LTI 1.3 Deep Linking in an application.
/// </summary>
public static class Startup
{
    /// <summary>
    /// Adds LTI 1.3 Platform Deep Linking services to the application.
    /// </summary>
    /// <param name="builder">The LTI 1.3 platform builder.</param>
    /// <returns>The LTI 1.3 platform builder for further configuration.</returns>
    public static Lti13PlatformBuilder AddLti13PlatformDeepLinking(this Lti13PlatformBuilder builder)
    {
        builder.Services.AddTransient<ILti13DeepLinkingUrlService, DeepLinkingUrlService>();

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

    /// <summary>
    /// Configures a custom implementation of the Deep Linking Data Service.
    /// </summary>
    /// <typeparam name="T">The implementation type of the ILti13DeepLinkingDataService interface.</typeparam>
    /// <param name="builder">The LTI 1.3 platform builder.</param>
    /// <param name="serviceLifetime">The lifetime of the service in the dependency injection container.</param>
    /// <returns>The LTI 1.3 platform builder for further configuration.</returns>
    public static Lti13PlatformBuilder WithLti13DeepLinkingDataService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13DeepLinkingDataService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingDataService), typeof(T), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Configures a custom implementation of the Deep Linking Config Service.
    /// </summary>
    /// <typeparam name="T">The implementation type of the ILti13DeepLinkingConfigService interface.</typeparam>
    /// <param name="builder">The LTI 1.3 platform builder.</param>
    /// <param name="serviceLifetime">The lifetime of the service in the dependency injection container.</param>
    /// <returns>The LTI 1.3 platform builder for further configuration.</returns>
    public static Lti13PlatformBuilder WithLti13DeepLinkingConfigService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13DeepLinkingConfigService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingConfigService), typeof(T), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Configures a custom implementation of the Deep Linking Handler.
    /// </summary>
    /// <typeparam name="T">The implementation type of the ILti13DeepLinkingHandler interface.</typeparam>
    /// <param name="builder">The LTI 1.3 platform builder.</param>
    /// <param name="serviceLifetime">The lifetime of the service in the dependency injection container.</param>
    /// <returns>The LTI 1.3 platform builder for further configuration.</returns>
    public static Lti13PlatformBuilder WithLti13DeepLinkingHandler<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13DeepLinkingHandler
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingHandler), typeof(T), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Configures the endpoints for LTI 1.3 Deep Linking.
    /// </summary>
    /// <param name="endpointRouteBuilder">The endpoint route builder.</param>
    /// <param name="configure">Optional function to configure endpoints.</param>
    /// <returns>The endpoint route builder for further configuration.</returns>
    public static IEndpointRouteBuilder UseLti13PlatformDeepLinking(this IEndpointRouteBuilder endpointRouteBuilder, Func<DeepLinkingEndpointsConfig, DeepLinkingEndpointsConfig>? configure = null)
    {
        const string OpenAPI_Tag = "LTI 1.3 Deep Linking";

        DeepLinkingEndpointsConfig config = new();
        config = configure?.Invoke(config) ?? config;

        _ = endpointRouteBuilder.MapPost(config.DeepLinkingResponseUrl,
            async ([FromForm] DeepLinkResponseRequest request, ContextId? contextId, ILogger<DeepLinkResponseRequest> logger, ILti13TokenConfigService tokenService, ILti13CoreDataService coreDataService, ILti13DeepLinkingDataService deepLinkingDataService, ILti13DeepLinkingConfigService deepLinkingService, ILti13DeepLinkingHandler deepLinkingHandler, CancellationToken cancellationToken) =>
            {
                const string DEEP_LINKING_SPEC = "https://www.imsglobal.org/spec/lti-dl/v2p0/#deep-linking-response-message";
                const string INVALID_REQUEST = "invalid_request";

                if (string.IsNullOrWhiteSpace(request.Jwt))
                {
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "JWT is required", Error_Uri = DEEP_LINKING_SPEC });
                }

                var jwt = new JsonWebToken(request.Jwt);
                var clientId = new ClientId(jwt.Issuer);

                var tool = await coreDataService.GetToolAsync(clientId, cancellationToken);
                if (tool?.Jwks == null)
                {
                    return Results.NotFound(new { Error = "invalid_client", Error_Description = "client_id is required", Error_Uri = DEEP_LINKING_SPEC });
                }

                if (!jwt.TryGetClaim("https://purl.imsglobal.org/spec/lti/claim/deployment_id", out var deploymentIdClaim))
                {
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "deployment_id is required", Error_Uri = DEEP_LINKING_SPEC });
                }

                var deployment = await coreDataService.GetDeploymentAsync(new DeploymentId(deploymentIdClaim.Value), cancellationToken);
                if (deployment == null || deployment.ClientId != tool.ClientId)
                {
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "deployment_id is invalid", Error_Uri = DEEP_LINKING_SPEC });
                }

                var tokenConfig = await tokenService.GetTokenConfigAsync(tool.ClientId, cancellationToken);

                var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(request.Jwt, new TokenValidationParameters
                {
                    IssuerSigningKeys = await tool.Jwks.GetKeysAsync(cancellationToken),
                    ValidAudience = tokenConfig.Issuer.OriginalString,
                    ValidIssuer = tool.ClientId.ToString()
                });

                if (!validatedToken.IsValid)
                {
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = validatedToken.Exception.Message, Error_Uri = DEEP_LINKING_SPEC });
                }

                if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/message_type", out var messageType) || (string)messageType != "LtiDeepLinkingResponse")
                {
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "message_type is invalid", Error_Uri = DEEP_LINKING_SPEC });
                }

                if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/version", out var version) || (string)version != "1.3.0")
                {
                    return Results.BadRequest(new LtiBadRequest { Error = INVALID_REQUEST, Error_Description = "version is invalid", Error_Uri = DEEP_LINKING_SPEC });
                }

                var deepLinkingConfig = await deepLinkingService.GetConfigAsync(tool.ClientId, cancellationToken);

                List<ContentItem> contentItems =
                    [
                        .. validatedToken.ClaimsIdentity.FindAll("https://purl.imsglobal.org/spec/lti-dl/claim/content_items")
                            .Select((x, ix) =>
                            {
                                var type = JsonDocument.Parse(x.Value).RootElement.GetProperty("type").GetString() ?? "unknown";
                                return (ContentItem)JsonSerializer.Deserialize(x.Value, deepLinkingConfig.ContentItemTypes[(tool.ClientId, type)])!;
                            })
                    ];

                var response = new DeepLinkResponse
                {
                    Data = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/data")?.Value,
                    Message = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/msg")?.Value,
                    Log = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/log")?.Value,
                    ErrorMessage = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errormsg")?.Value,
                    ErrorLog = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errorlog")?.Value,
                    ContentItems = contentItems,
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
                        if (ci.Type == ContentItemType.LtiResourceLink)
                        {
                            if (ci is not LtiResourceLinkContentItem ltiResourceLinkItem)
                            {
                                ltiResourceLinkItem = JsonSerializer.Deserialize<LtiResourceLinkContentItem>(JsonSerializer.Serialize(ci))!;
                            }

                            var id = await deepLinkingDataService.SaveResourceLinkAsync(deployment.Id, contextId, ltiResourceLinkItem);

                            if (deepLinkingConfig.AcceptLineItem == true && contextId != null && ltiResourceLinkItem?.LineItem != null)
                            {
                                await deepLinkingDataService.SaveLineItemAsync(new LineItem
                                {
                                    Id = LineItemId.Empty,
                                    DeploymentId = deployment.Id,
                                    ContextId = contextId.GetValueOrDefault(),
                                    Label = ltiResourceLinkItem.LineItem!.Label ?? ltiResourceLinkItem.Title ?? ltiResourceLinkItem.Type,
                                    ScoreMaximum = ltiResourceLinkItem.LineItem.ScoreMaximum,
                                    GradesReleased = ltiResourceLinkItem.LineItem.GradesReleased,
                                    Tag = ltiResourceLinkItem.LineItem.Tag,
                                    ResourceId = ltiResourceLinkItem.LineItem.ResourceId,
                                    ResourceLinkId = id,
                                    StartDateTime = ltiResourceLinkItem.Submission?.StartDateTime?.UtcDateTime,
                                    EndDateTime = ltiResourceLinkItem.Submission?.EndDateTime?.UtcDateTime
                                },
                                cancellationToken);
                            }
                        }
                        else
                        {
                            await deepLinkingDataService.SaveContentItemAsync(deployment.Id, contextId, ci, cancellationToken);
                        }
                    });

                    await Task.WhenAll(saveTasks);
                }

                return await deepLinkingHandler.HandleResponseAsync(tool.ClientId, deployment.Id, contextId, response, cancellationToken);
            })
            .WithName(RouteNames.DEEP_LINKING_RESPONSE)
            .DisableAntiforgery()
            .Produces<LtiBadRequest>(StatusCodes.Status400BadRequest)
            .Produces<LtiBadRequest>(StatusCodes.Status404NotFound)
            .WithGroupName(OpenApi.GroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Handles the deep linking response from the tool.")
            .WithDescription("After a user selects items to be deep linked, the tool will return the user to this endpoint with the selected items. This endpoint will validate the request and handle the resulting items. Not all possible results are shown as the results will be determined by how it is handled.");

        return endpointRouteBuilder;
    }
}

/// <summary>
/// Represents a request for deep linking response containing a JWT token.
/// </summary>
/// <param name="Jwt">The JWT token containing the deep linking response data.</param>
internal record DeepLinkResponseRequest(string? Jwt);
