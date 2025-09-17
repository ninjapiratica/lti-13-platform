using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Models;
using System.Text;
using System.Text.Json;
using System.Web;

namespace NP.Lti13Platform.Core.Services;

/// <summary>
/// Defines the contract for a helper service that builds LTI 1.3 URLs.
/// </summary>
public interface IUrlServiceHelper
{
    /// <summary>
    /// Gets the resource link initiation URL.
    /// </summary>
    /// <param name="tool">The tool.</param>
    /// <param name="deploymentId">The deployment ID.</param>
    /// <param name="contextId">The context ID.</param>
    /// <param name="resourceLink">The resource link.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="isAnonymous">A value indicating whether the user is anonymous.</param>
    /// <param name="actualUserId">The actual user ID (if impersonating).</param>
    /// <param name="launchPresentation">The launch presentation override.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resource link initiation URL.</returns>
    Task<Uri> GetResourceLinkInitiationUrlAsync(Tool tool, string deploymentId, string contextId, ResourceLink resourceLink, string userId, bool isAnonymous, string? actualUserId = null, LaunchPresentationOverride? launchPresentation = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets a URL for an LTI message.
    /// </summary>
    /// <param name="messageType">The message type.</param>
    /// <param name="tool">The tool.</param>
    /// <param name="deploymentId">The deployment ID.</param>
    /// <param name="targetLinkUri">The target link URI.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="isAnonymous">A value indicating whether the user is anonymous.</param>
    /// <param name="actualUserId">The actual user ID (if impersonating).</param>
    /// <param name="contextId">The context ID.</param>
    /// <param name="resourceLinkId">The resource link ID.</param>
    /// <param name="messageHint">The message hint.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The URL.</returns>
    Task<Uri> GetUrlAsync(string messageType, Tool tool, string deploymentId, Uri targetLinkUri, string userId, bool isAnonymous, string? actualUserId = null, string? contextId = null, string? resourceLinkId = null, string? messageHint = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the login hint.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="actualUserId">The actual user ID (if impersonating).</param>
    /// <param name="isAnonymous">A value indicating whether the user is anonymous.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The login hint.</returns>
    Task<string> GetLoginHintAsync(string userId, string? actualUserId, bool isAnonymous, CancellationToken cancellationToken = default);
    /// <summary>
    /// Parses the login hint.
    /// </summary>
    /// <param name="loginHint">The login hint.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parsed login hint components.</returns>
    Task<(string UserId, string? ActualUserId, bool IsAnonymous)> ParseLoginHintAsync(string loginHint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the LTI message hint.
    /// </summary>
    /// <param name="MessageType">The message type.</param>
    /// <param name="DeploymentId">The deployment ID.</param>
    /// <param name="ContextId">The context ID.</param>
    /// <param name="ResourceLinkId">The resource link ID.</param>
    /// <param name="messageHint">The message hint.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The LTI message hint.</returns>
    Task<string> GetLtiMessageHintAsync(string MessageType, string DeploymentId, string? ContextId, string? ResourceLinkId, string? messageHint, CancellationToken cancellationToken = default);
    /// <summary>
    /// Parses the LTI message hint.
    /// </summary>
    /// <param name="messageHint">The message hint.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parsed LTI message hint components.</returns>
    Task<(string MessageType, string DeploymentId, string? ContextId, string? ResourceLinkId, string? MessageHint)> ParseLtiMessageHintAsync(string messageHint, CancellationToken cancellationToken = default);
}

/// <summary>
/// A helper service that builds LTI 1.3 URLs.
/// </summary>
/// <param name="tokenService">The token service.</param>
public class UrlServiceHelper(ILti13TokenConfigService tokenService) : IUrlServiceHelper
{
    /// <inheritdoc />
    public async Task<Uri> GetResourceLinkInitiationUrlAsync(Tool tool, string deploymentId, string contextId, ResourceLink resourceLink, string userId, bool isAnonymous, string? actualUserId = null, LaunchPresentationOverride? launchPresentation = null, CancellationToken cancellationToken = default)
        => await GetUrlAsync(
            Lti13MessageType.LtiResourceLinkRequest,
            tool,
            deploymentId,
            resourceLink.Url ?? tool.LaunchUrl,
            userId,
            isAnonymous,
            actualUserId,
            contextId,
            resourceLink.Id,
            Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(launchPresentation))),
            cancellationToken);

    /// <inheritdoc />
    public async Task<Uri> GetUrlAsync(
        string messageType,
        Tool tool,
        string deploymentId,
        Uri targetLinkUri,
        string userId,
        bool isAnonymous,
        string? actualUserId = null,
        string? contextId = null,
        string? resourceLinkId = null,
        string? messageHint = null,
        CancellationToken cancellationToken = default)
    {
        var builder = new UriBuilder(tool.OidcInitiationUrl);

        var query = HttpUtility.ParseQueryString(builder.Query);
        query.Add("iss", (await tokenService.GetTokenConfigAsync(tool.ClientId, cancellationToken)).Issuer.ToString());
        query.Add("login_hint", await GetLoginHintAsync(userId, actualUserId, isAnonymous, cancellationToken));
        query.Add("target_link_uri", targetLinkUri.ToString());
        query.Add("client_id", tool.ClientId.ToString());
        query.Add("lti_message_hint", await GetLtiMessageHintAsync(messageType, deploymentId, contextId, resourceLinkId, messageHint, cancellationToken));
        query.Add("lti_deployment_id", deploymentId);
        builder.Query = query.ToString();

        return builder.Uri;
    }

    /// <inheritdoc />
    public async Task<string> GetLoginHintAsync(string userId, string? actualUserId, bool isAnonymous, CancellationToken cancellationToken = default) =>
        await Task.FromResult($"{userId}|{(isAnonymous ? "1" : string.Empty)}|{actualUserId}");

    /// <inheritdoc />
    public async Task<(string UserId, string? ActualUserId, bool IsAnonymous)> ParseLoginHintAsync(string loginHint, CancellationToken cancellationToken = default) =>
        await Task.FromResult(loginHint.Split('|', 3) is [var userId, var isAnonymousString, var actualUserId] ?
            (userId, actualUserId, !string.IsNullOrWhiteSpace(isAnonymousString)) :
            (string.Empty, null, false));

    /// <inheritdoc />
    public async Task<string> GetLtiMessageHintAsync(string messageType, string deploymentId, string? contextId, string? resourceLinkId, string? messageHint, CancellationToken cancellationToken = default) =>
        await Task.FromResult($"{messageType}|{deploymentId}|{contextId}|{resourceLinkId}|{messageHint}");

    /// <inheritdoc />
    public async Task<(string MessageType, string DeploymentId, string? ContextId, string? ResourceLinkId, string? MessageHint)> ParseLtiMessageHintAsync(string messageHint, CancellationToken cancellationToken = default) =>
        await Task.FromResult(messageHint.Split('|', 5) is [var messageType, var deploymentId, var contextId, var resourceLinkId, var messageHintString] ?
            (messageType, deploymentId, contextId, resourceLinkId, messageHintString) :
            (string.Empty, string.Empty, null, null, null));
}
