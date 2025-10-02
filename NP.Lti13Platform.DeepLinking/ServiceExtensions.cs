using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using System.Text;
using System.Text.Json;

namespace NP.Lti13Platform.DeepLinking;

/// <summary>
/// Provides extension methods for Deep Linking services.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Gets a Deep Link initiation URL asynchronously.
    /// </summary>
    /// <param name="service">The URL service helper.</param>
    /// <param name="tool">The tool to use for deep linking.</param>
    /// <param name="deploymentId">The deployment identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="isAnonymous">A value indicating whether the user is anonymous.</param>
    /// <param name="actualUserId">The actual user identifier, if different from userId.</param>
    /// <param name="contextId">The context identifier.</param>
    /// <param name="deepLinkSettings">Optional deep link settings override.</param>
    /// <param name="launchPresentation">Optional launch presentation override.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deep link initiation URL.</returns>
    public static async Task<Uri> GetDeepLinkInitiationUrlAsync(
        this IUrlServiceHelper service,
        Tool tool,
        DeploymentId deploymentId,
        UserId userId,
        bool isAnonymous,
        UserId? actualUserId = null,
        ContextId? contextId = null,
        DeepLinkSettingsOverride? deepLinkSettings = null,
        LaunchPresentationOverride? launchPresentation = null,
        CancellationToken cancellationToken = default)
        => await service.GetUrlAsync(
            Lti13MessageType.LtiDeepLinkingRequest,
            tool,
            deploymentId,
            tool.DeepLinkUrl,
            userId,
            isAnonymous,
            actualUserId,
            contextId,
            resourceLinkId: null,
            Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(deepLinkSettings) + "|" + JsonSerializer.Serialize(launchPresentation))),
            cancellationToken);
}
