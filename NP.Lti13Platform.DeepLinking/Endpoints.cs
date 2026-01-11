using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.OpenApi;
using NP.Lti13Platform.Core.Services;
using NP.Lti13Platform.DeepLinking.Configs;
using NP.Lti13Platform.DeepLinking.Constants;
using NP.Lti13Platform.DeepLinking.Models;
using NP.Lti13Platform.DeepLinking.Services;
using System.Text.Json;

namespace NP.Lti13Platform.DeepLinking;

/// <summary>
/// Provides extension methods to configure LTI 1.3 Deep Linking in an application.
/// </summary>
public static class Endpoints
{
    /// <summary>
    /// Configures the endpoints for LTI 1.3 Deep Linking.
    /// </summary>
    /// <param name="endpointRouteBuilder">The endpoint route builder.</param>
    /// <param name="configure">Optional function to configure endpoints.</param>
    /// <returns>The endpoint route builder for further configuration.</returns>
    public static IEndpointRouteBuilder UseLti13PlatformDeepLinking(this IEndpointRouteBuilder endpointRouteBuilder, Func<EndpointsConfig, EndpointsConfig>? configure = null)
    {
        const string OpenAPI_Tag = "LTI 1.3 Deep Linking";

        EndpointsConfig config = new();
        config = configure?.Invoke(config) ?? config;

        _ = endpointRouteBuilder.MapPost(config.DeepLinkingResponseUrl,
            async ([FromForm] DeepLinkingResponseRequest request,
                ContextId? contextId,
                ILogger<DeepLinkingResponseRequest> logger,
                ITokenConfigService tokenService,
                ICoreDataService coreDataService,
                IDeepLinkingResponseDataService deepLinkingResponseDataService,
                IDeepLinkingConfigService deepLinkingService,
                IDeepLinkingResponseHandler deepLinkingResponseHandler,
                CancellationToken cancellationToken) =>
            {
                const string DEEP_LINKING_SPEC = "https://www.imsglobal.org/spec/lti-dl/v2p0/#deep-linking-response-message";
                const string INVALID_REQUEST = "invalid_request";

                if (string.IsNullOrWhiteSpace(request.Jwt))
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_REQUEST,
                        Error_Description = "JWT is required",
                        Error_Uri = DEEP_LINKING_SPEC
                    });
                }

                var jwt = new JsonWebToken(request.Jwt);
                var clientId = new ClientId(jwt.Issuer);

                var tool = await coreDataService.GetToolAsync(clientId, cancellationToken);
                if (tool?.Jwks == null)
                {
                    return Results.NotFound(new
                    {
                        Error = "invalid_client",
                        Error_Description = "client_id is required",
                        Error_Uri = DEEP_LINKING_SPEC
                    });
                }

                if (!jwt.TryGetClaim("https://purl.imsglobal.org/spec/lti/claim/deployment_id", out var deploymentIdClaim))
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_REQUEST,
                        Error_Description = "deployment_id is required",
                        Error_Uri = DEEP_LINKING_SPEC
                    });
                }

                var deployment = await deepLinkingResponseDataService.GetDeploymentAsync(new DeploymentId(deploymentIdClaim.Value), cancellationToken);
                if (deployment == null || deployment.ClientId != tool.ClientId)
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_REQUEST,
                        Error_Description = "deployment_id is invalid",
                        Error_Uri = DEEP_LINKING_SPEC
                    });
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
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_REQUEST,
                        Error_Description = validatedToken.Exception.Message,
                        Error_Uri = DEEP_LINKING_SPEC
                    });
                }

                if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/message_type", out var messageType) || (string)messageType != "LtiDeepLinkingResponse")
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_REQUEST,
                        Error_Description = "message_type is invalid",
                        Error_Uri = DEEP_LINKING_SPEC
                    });
                }

                if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/version", out var version) || (string)version != "1.3.0")
                {
                    return Results.BadRequest(new Lti13BadRequest
                    {
                        Error = INVALID_REQUEST,
                        Error_Description = "version is invalid",
                        Error_Uri = DEEP_LINKING_SPEC
                    });
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

                var response = new DeepLinkingResponse
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
                            if (ci is not LtiResourceLinkContentItem ltiResourceLinkContentItem)
                            {
                                ltiResourceLinkContentItem = JsonSerializer.Deserialize<LtiResourceLinkContentItem>(JsonSerializer.Serialize(ci))!;
                            }

                            var id = await deepLinkingResponseDataService.SaveResourceLinkAsync(deployment.Id, contextId, ltiResourceLinkContentItem);

                            if (deepLinkingConfig.AcceptLineItem == true && contextId != null && ltiResourceLinkContentItem?.LineItem != null)
                            {
                                await deepLinkingResponseDataService.SaveLineItemAsync(new LineItem
                                {
                                    Id = LineItemId.Empty,
                                    DeploymentId = deployment.Id,
                                    ContextId = contextId.GetValueOrDefault(),
                                    Label = ltiResourceLinkContentItem.LineItem.Label ?? ltiResourceLinkContentItem.Title ?? ltiResourceLinkContentItem.Type,
                                    ScoreMaximum = ltiResourceLinkContentItem.LineItem.ScoreMaximum,
                                    GradesReleased = ltiResourceLinkContentItem.LineItem.GradesReleased,
                                    Tag = ltiResourceLinkContentItem.LineItem.Tag,
                                    ResourceId = ltiResourceLinkContentItem.LineItem.ResourceId,
                                    ResourceLinkId = id,
                                    StartDateTime = ltiResourceLinkContentItem.Submission?.StartDateTime?.UtcDateTime,
                                    EndDateTime = ltiResourceLinkContentItem.Submission?.EndDateTime?.UtcDateTime
                                },
                                cancellationToken);
                            }
                        }
                        else
                        {
                            await deepLinkingResponseDataService.SaveContentItemAsync(deployment.Id, contextId, ci, cancellationToken);
                        }
                    });

                    await Task.WhenAll(saveTasks);
                }

                return await deepLinkingResponseHandler.HandleResponseAsync(tool.ClientId, deployment.Id, contextId, response, cancellationToken);
            })
            .WithName(RouteNames.DEEP_LINKING_RESPONSE)
            .DisableAntiforgery()
            .Produces<Lti13BadRequest>(StatusCodes.Status400BadRequest)
            .Produces<Lti13BadRequest>(StatusCodes.Status404NotFound)
            .WithGroupName(Lti13OpenApi.GroupName)
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
internal record DeepLinkingResponseRequest(string? Jwt);

/// <summary>
/// Represents a response from a deep linking tool as defined in the IMS Global LTI Deep Linking specification.
/// The Deep Linking Response Message is sent from the Tool to the Platform after the user has finished
/// selecting and/or configuring content in the Tool.
/// </summary>
public record DeepLinkingResponse
{
    /// <summary>
    /// The opaque data value from the deep linking request. This value MUST be opaque to the Tool, and used as-is from the request, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// An optional plain text message that the Platform MAY display to the user as a result of the deep linking, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// An optional plain text message that the Platform MAY include in any logs or analytics, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public string? Log { get; set; }

    /// <summary>
    /// An optional plain text error message that the Platform MAY display to the user as a result of the deep linking, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// An optional plain text error message that the Platform MAY include in any logs or analytics, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public string? ErrorLog { get; set; }

    /// <summary>
    /// An array of Content Items as defined in the IMS Global LTI Deep Linking specification. These items represent the content that the user has selected or created in the tool to be used within the Platform.
    /// </summary>
    public IEnumerable<ContentItem> ContentItems { get; set; } = [];
}

/// <summary>
/// Represents override settings for deep linking functionality.
/// </summary>
public record DeepLinkingSettingsOverride
{
    /// <summary>
    /// Gets or sets the URL where the platform should return the deep linking response.
    /// </summary>
    public string? DeepLinkReturnUrl { get; set; }

    /// <summary>
    /// Gets or sets the content types that are acceptable for deep linking.
    /// </summary>
    public IEnumerable<string>? AcceptTypes { get; set; }

    /// <summary>
    /// Gets or sets the document targets that are acceptable for presenting deep linked content.
    /// </summary>
    public IEnumerable<string>? AcceptPresentationDocumentTargets { get; set; }

    /// <summary>
    /// Gets or sets the media types that are acceptable for deep linking.
    /// </summary>
    public IEnumerable<string>? AcceptMediaTypes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether multiple content items can be selected.
    /// </summary>
    public bool? AcceptMultiple { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether line items can be accepted.
    /// </summary>
    public bool? AcceptLineItem { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether content items should be automatically created.
    /// </summary>
    public bool? AutoCreate { get; set; }

    /// <summary>
    /// Gets or sets the title to display for the deep linking interface.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the descriptive text to display for the deep linking interface.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets opaque data that will be passed back unchanged in the deep linking response.
    /// </summary>
    public string? Data { get; set; }
}
