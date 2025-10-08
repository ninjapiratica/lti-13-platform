using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Models;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;

namespace NP.Lti13Platform.Core.Services;

/// <summary>
/// Defines the contract for a helper service that builds LTI 1.3 URLs.
/// </summary>
public interface IUrlService
{
    /// <summary>
    /// Gets the resource link initiation URL.
    /// </summary>
    /// <param name="resourceLinkId">The resource link ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="isAnonymous">A value indicating whether the user is anonymous.</param>
    /// <param name="actualUserId">The actual user ID (if impersonating).</param>
    /// <param name="launchPresentation">The launch presentation override.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resource link initiation URL.</returns>
    Task<LtiLaunch> GetResourceLinkInitiationUrlAsync(ResourceLinkId resourceLinkId, UserId userId, bool isAnonymous = false, UserId? actualUserId = null, LaunchPresentationOverride? launchPresentation = null, CancellationToken cancellationToken = default);

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
    /// <returns>The Lti Launch values.</returns>
    Task<LtiLaunch> GetUrlAsync(string messageType, Tool tool, DeploymentId deploymentId, Uri targetLinkUri, UserId userId, bool isAnonymous, UserId? actualUserId = null, ContextId? contextId = null, ResourceLinkId? resourceLinkId = null, string? messageHint = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the login hint.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="actualUserId">The actual user ID (if impersonating).</param>
    /// <param name="isAnonymous">A value indicating whether the user is anonymous.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The login hint.</returns>
    Task<string> GetLoginHintAsync(UserId userId, UserId? actualUserId, bool isAnonymous, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses the login hint.
    /// </summary>
    /// <param name="loginHint">The login hint.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parsed login hint components.</returns>
    Task<(UserId UserId, UserId? ActualUserId, bool IsAnonymous)> ParseLoginHintAsync(string loginHint, CancellationToken cancellationToken = default);

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
    Task<string> GetLtiMessageHintAsync(string MessageType, DeploymentId DeploymentId, ContextId? ContextId, ResourceLinkId? ResourceLinkId, string? messageHint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses the LTI message hint.
    /// </summary>
    /// <param name="messageHint">The message hint.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parsed LTI message hint components.</returns>
    Task<(string MessageType, DeploymentId DeploymentId, ContextId? ContextId, ResourceLinkId? ResourceLinkId, string? MessageHint)> ParseLtiMessageHintAsync(string messageHint, CancellationToken cancellationToken = default);
}

/// <summary>
/// A helper service that builds LTI 1.3 URLs.
/// </summary>
/// <param name="tokenService">The token service.</param>
/// <param name="coreDataService">The core data service.</param>
public class UrlService(ILti13TokenConfigService tokenService, ILti13CoreDataService coreDataService) : IUrlService
{
    /// <inheritdoc />
    public async Task<LtiLaunch> GetResourceLinkInitiationUrlAsync(ResourceLinkId resourceLinkId, UserId userId, bool isAnonymous, UserId? actualUserId = null, LaunchPresentationOverride? launchPresentation = null, CancellationToken cancellationToken = default)
    {
        var resourceLink = (await coreDataService.GetResourceLinkAsync(resourceLinkId, cancellationToken))!;
        var deployment = (await coreDataService.GetDeploymentAsync(resourceLink!.DeploymentId, cancellationToken))!;
        var tool = (await coreDataService.GetToolAsync(deployment.ClientId, cancellationToken))!;

        return await GetUrlAsync(
            Lti13MessageType.LtiResourceLinkRequest,
            tool,
            resourceLink.DeploymentId,
            resourceLink.Url ?? tool.LaunchUrl,
            userId,
            isAnonymous,
            actualUserId,
            resourceLink.ContextId,
            resourceLink.Id,
            Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(launchPresentation))),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LtiLaunch> GetUrlAsync(
        string messageType,
        Tool tool,
        DeploymentId deploymentId,
        Uri targetLinkUri,
        UserId userId,
        bool isAnonymous = false,
        UserId? actualUserId = null,
        ContextId? contextId = null,
        ResourceLinkId? resourceLinkId = null,
        string? messageHint = null,
        CancellationToken cancellationToken = default) => new LtiLaunch(
            tool,
            (await tokenService.GetTokenConfigAsync(tool.ClientId, cancellationToken)).Issuer,
            targetLinkUri,
            tool.ClientId,
            deploymentId,
            await GetLoginHintAsync(userId, actualUserId, isAnonymous, cancellationToken),
            await GetLtiMessageHintAsync(messageType, deploymentId, contextId, resourceLinkId, messageHint, cancellationToken));

    /// <inheritdoc />
    public async Task<string> GetLoginHintAsync(UserId userId, UserId? actualUserId, bool isAnonymous, CancellationToken cancellationToken = default) =>
        await Task.FromResult($"{userId}|{(isAnonymous ? "1" : string.Empty)}|{actualUserId}");

    /// <inheritdoc />
    public async Task<(UserId UserId, UserId? ActualUserId, bool IsAnonymous)> ParseLoginHintAsync(string loginHint, CancellationToken cancellationToken = default) =>
        await Task.FromResult(loginHint.Split('|', 3) is [var userId, var isAnonymousString, var actualUserId] ?
            (new UserId(userId), (UserId?)new UserId(actualUserId), !string.IsNullOrWhiteSpace(isAnonymousString)) :
            (UserId.Empty, null, false));

    /// <inheritdoc />
    public async Task<string> GetLtiMessageHintAsync(string messageType, DeploymentId deploymentId, ContextId? contextId, ResourceLinkId? resourceLinkId, string? messageHint, CancellationToken cancellationToken = default) =>
        await Task.FromResult($"{messageType}|{deploymentId}|{contextId}|{resourceLinkId}|{messageHint}");

    /// <inheritdoc />
    public async Task<(string MessageType, DeploymentId DeploymentId, ContextId? ContextId, ResourceLinkId? ResourceLinkId, string? MessageHint)> ParseLtiMessageHintAsync(string messageHint, CancellationToken cancellationToken = default) =>
        await Task.FromResult(messageHint.Split('|', 5) is [var messageType, var deploymentId, var contextId, var resourceLinkId, var messageHintString] ?
            (messageType, 
                new DeploymentId(deploymentId), 
                string.IsNullOrWhiteSpace(contextId) ? (ContextId?)null : new ContextId(contextId), 
                string.IsNullOrWhiteSpace(resourceLinkId) ? (ResourceLinkId?)null : new ResourceLinkId(resourceLinkId),
                messageHintString) :
            (string.Empty, DeploymentId.Empty, null, null, null));
}

/// <summary>
/// Represents the data required to initiate an LTI (Learning Tools Interoperability) launch flow.
/// </summary>
/// <remarks>This record encapsulates the parameters necessary for initiating an OpenID Connect (OIDC) login flow
/// as part of an LTI launch. It includes details such as the tool configuration, issuer, target link URI, client ID,
/// deployment ID, and hints for login and LTI message handling.</remarks>
/// <param name="Tool">The tool configuration to launch. This contains the tool's OIDC initiation URL, launch URL, client identifier, and other metadata required to initiate an LTI launch.</param>
/// <param name="Issuer">The platform issuer URI. This value is used as the 'iss' parameter in the OIDC authentication request to identify the platform to the tool.</param>
/// <param name="TargetLinkUri">The target link URI. This is the destination within the tool that the platform requests the user be directed to after a successful OIDC login (the 'target_link_uri' parameter).</param>
/// <param name="ClientId">The tool's OAuth2/OpenID client identifier. This value is provided as the 'client_id' parameter in the OIDC request.</param>
/// <param name="DeploymentId">The deployment identifier for the platform-tool installation. This value identifies the specific integration/installation and is sent as 'lti_deployment_id'.</param>
/// <param name="LoginHint">The computed login hint. This value is transmitted as the 'login_hint' parameter to correlate the login to a platform user (it may encode impersonation and anonymity flags).</param>
/// <param name="LtiMessageHint">The LTI message hint. This value is sent as 'lti_message_hint' to convey the LTI message context (message type, deployment, context, resource link, and optional message hint) to the tool.</param>
public record LtiLaunch(Tool Tool, Uri Issuer, Uri TargetLinkUri, ClientId ClientId, DeploymentId DeploymentId, string LoginHint, string LtiMessageHint)
{
    /// <summary>
    /// Constructs a URI with query parameters required for OIDC initiation.
    /// </summary>
    /// <remarks>The resulting URI includes the base OIDC initiation URL and appends query parameters such as 
    /// issuer, login hint, target link URI, client ID, LTI message hint, and LTI deployment ID.  This method is
    /// typically used to initiate an OpenID Connect (OIDC) login flow.</remarks>
    /// <returns>A <see cref="Uri"/> representing the OIDC initiation URL with the required query parameters.</returns>
    public Uri AsUri()
    {
        var builder = new UriBuilder(Tool.OidcInitiationUrl);

        var query = HttpUtility.ParseQueryString(builder.Query);
        query.Add("iss", Issuer.OriginalString);
        query.Add("login_hint", LoginHint);
        query.Add("target_link_uri", TargetLinkUri.OriginalString);
        query.Add("client_id", ClientId.ToString());
        query.Add("lti_message_hint", LtiMessageHint);
        query.Add("lti_deployment_id", DeploymentId.ToString());
        builder.Query = query.ToString();

        return builder.Uri;
    }

    /// <summary>
    /// Generates an HTML form string for initiating an OpenID Connect (OIDC) login flow.
    /// </summary>
    /// <remarks>The generated form is intended to be used in scenarios where an OIDC login flow needs to be
    /// initiated via a POST request. The form includes a `noscript` block with a submit button to ensure
    /// functionality in environments where JavaScript is disabled.</remarks>
    /// <param name="formId">The ID to assign to the generated HTML form. This value is HTML-encoded to ensure safety. Should be used for submitting the form via javascript.</param>
    /// <returns>A string containing the HTML representation of a form configured for OIDC login initiation. The form includes
    /// hidden input fields for required parameters such as issuer, login hint, and client ID.</returns>
    public string AsForm(string formId) => $@"
<form id=""{HtmlEncoder.Default.Encode(formId)}"" action=""{HtmlEncoder.Default.Encode(Tool.OidcInitiationUrl.OriginalString)}"" method=""post"">
  <input type=""hidden"" name=""iss"" value=""{HtmlEncoder.Default.Encode(Issuer.OriginalString)}"" />
  <input type=""hidden"" name=""login_hint"" value=""{HtmlEncoder.Default.Encode(LoginHint)}"" />
  <input type=""hidden"" name=""target_link_uri"" value=""{HtmlEncoder.Default.Encode(TargetLinkUri.OriginalString)}"" />
  <input type=""hidden"" name=""client_id"" value=""{HtmlEncoder.Default.Encode(ClientId.ToString())}"" />
  <input type=""hidden"" name=""lti_message_hint"" value=""{HtmlEncoder.Default.Encode(LtiMessageHint)}"" />
  <input type=""hidden"" name=""lti_deployment_id"" value=""{HtmlEncoder.Default.Encode(DeploymentId.ToString())}"" />
  <noscript><button type=""submit"">Continue</button></noscript>
</form>".Trim();
}