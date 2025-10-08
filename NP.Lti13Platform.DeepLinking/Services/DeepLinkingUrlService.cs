using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using System.Text;
using System.Text.Json;

namespace NP.Lti13Platform.DeepLinking.Services;

/// <summary>
/// Service for generating LTI 1.3 deep link initiation URLs.
/// </summary>
/// <remarks>
/// Implementations produce an <see cref="LtiLaunch"/> that contains the OIDC initiation parameters
/// required to start a deep linking flow with a Tool configured in the platform.
/// </remarks>
public interface IDeepLinkingUrlService
{
    /// <summary>
    /// Gets a Deep Link initiation URL asynchronously.
    /// </summary>
    /// <param name="deploymentId">The deployment identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="isAnonymous">A value indicating whether the user is anonymous.</param>
    /// <param name="deepLinkUrl">The deep link URL. If null, the tool's launch URL will be used.</param>
    /// <param name="actualUserId">The actual user identifier, if different from userId.</param>
    /// <param name="contextId">The context identifier.</param>
    /// <param name="deepLinkSettings">Optional deep link settings override.</param>
    /// <param name="launchPresentation">Optional launch presentation override.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deep link initiation URL.</returns>
    Task<LtiLaunch> GetDeepLinkInitiationUrlAsync(
        DeploymentId deploymentId,
        UserId userId,
        bool isAnonymous = false,
        Uri? deepLinkUrl = null,
        UserId? actualUserId = null,
        ContextId? contextId = null,
        DeepLinkSettingsOverride? deepLinkSettings = null,
        LaunchPresentationOverride? launchPresentation = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides methods for generating deep link initiation URLs in the context of LTI 1.3.
/// </summary>
/// <remarks>
/// This class extends URL-building to support the creation of deep link initiation
/// requests for LTI 1.3-compliant tools. It relies on the provided <see cref="ILti13CoreDataService"/>
/// to retrieve deployment and tool information and on <see cref="IUrlService"/> to build the final URL.
/// </remarks>
/// <param name="coreDataService">The core data service used to read tools and deployments.</param>
/// <param name="urlService">The URL service used to build LTI messages and hints.</param>
public class DeepLinkingUrlService(ILti13CoreDataService coreDataService, IUrlService urlService) : IDeepLinkingUrlService
{
    /// <inheritdoc />
    public async Task<LtiLaunch> GetDeepLinkInitiationUrlAsync(
        DeploymentId deploymentId,
        UserId userId,
        bool isAnonymous = false,
        Uri? deepLinkUrl = null,
        UserId? actualUserId = null,
        ContextId? contextId = null,
        DeepLinkSettingsOverride? deepLinkSettings = null,
        LaunchPresentationOverride? launchPresentation = null,
        CancellationToken cancellationToken = default)
    {
        var deployment = (await coreDataService.GetDeploymentAsync(deploymentId, cancellationToken))!;
        var tool = (await coreDataService.GetToolAsync(deployment.ClientId, cancellationToken))!;

        return await urlService.GetUrlAsync(
            Lti13MessageType.LtiDeepLinkingRequest,
            tool,
            deploymentId,
            deepLinkUrl ?? tool.LaunchUrl,
            userId,
            isAnonymous,
            actualUserId,
            contextId,
            resourceLinkId: null,
            Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(deepLinkSettings) + "|" + JsonSerializer.Serialize(launchPresentation))),
            cancellationToken);
    }
}
