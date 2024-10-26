using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Models;
using System.Text;
using System.Text.Json;
using System.Web;

namespace NP.Lti13Platform.Core.Services
{
    public interface IUrlServiceHelper
    {
        Task<Uri> GetResourceLinkInitiationUrlAsync(Tool tool, string deploymentId, string contextId, ResourceLink resourceLink, string userId, bool isAnonymous, string? actualUserId = null, LaunchPresentationOverride? launchPresentation = null, CancellationToken cancellationToken = default);
        Task<Uri> GetUrlAsync(string messageType, Tool tool, string deploymentId, string targetLinkUri, string userId, bool isAnonymous, string? actualUserId = null, string? contextId = null, string? resourceLinkId = null, string? messageHint = null, CancellationToken cancellationToken = default);

        Task<string> GetLoginHintAsync(string userId, string? actualUserId, bool isAnonymous, CancellationToken cancellationToken = default);
        Task<(string UserId, string? ActualUserId, bool IsAnonymous)> ParseLoginHintAsync(string loginHint, CancellationToken cancellationToken = default);

        Task<string> GetLtiMessageHintAsync(string MessageType, string DeploymentId, string? ContextId, string? ResourceLinkId, string? messageHint, CancellationToken cancellationToken = default);
        Task<(string MessageType, string DeploymentId, string? ContextId, string? ResourceLinkId, string? MessageHint)> ParseLtiMessageHintAsync(string messageHint, CancellationToken cancellationToken = default);
    }

    public class UrlServiceHelper(ITokenService tokenService) : IUrlServiceHelper
    {
        public async Task<Uri> GetResourceLinkInitiationUrlAsync(Tool tool, string deploymentId, string contextId, ResourceLink resourceLink, string userId, bool isAnonymous, string? actualUserId = null, LaunchPresentationOverride? launchPresentation = null, CancellationToken cancellationToken = default)
            => await GetUrlAsync(
                Lti13MessageType.LtiResourceLinkRequest,
                tool,
                deploymentId,
                string.IsNullOrWhiteSpace(resourceLink.Url) ? tool.LaunchUrl : resourceLink.Url,
                userId,
                isAnonymous,
                actualUserId,
                contextId,
                resourceLink.Id,
                Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(launchPresentation))),
                cancellationToken);

        public async Task<Uri> GetUrlAsync(
            string messageType,
            Tool tool,
            string deploymentId,
            string targetLinkUri,
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
            query.Add("iss", (await tokenService.GetTokenConfigAsync(tool.ClientId, cancellationToken)).Issuer);
            query.Add("login_hint", await GetLoginHintAsync(userId, actualUserId, isAnonymous, cancellationToken));
            query.Add("target_link_uri", targetLinkUri);
            query.Add("client_id", tool.ClientId.ToString());
            query.Add("lti_message_hint", await GetLtiMessageHintAsync(messageType, deploymentId, contextId, resourceLinkId, messageHint, cancellationToken));
            query.Add("lti_deployment_id", deploymentId);
            builder.Query = query.ToString();

            return builder.Uri;
        }
        public async Task<string> GetLoginHintAsync(string userId, string? actualUserId, bool isAnonymous, CancellationToken cancellationToken = default) =>
            await Task.FromResult($"{userId}|{(isAnonymous ? "1" : string.Empty)}|{actualUserId}");

        public async Task<(string UserId, string? ActualUserId, bool IsAnonymous)> ParseLoginHintAsync(string loginHint, CancellationToken cancellationToken = default) =>
            await Task.FromResult(loginHint.Split('|', 3) is [var userId, var isAnonymousString, var actualUserId] ?
                (userId, actualUserId, !string.IsNullOrWhiteSpace(isAnonymousString)) :
                (string.Empty, null, false));

        public async Task<string> GetLtiMessageHintAsync(string messageType, string deploymentId, string? contextId, string? resourceLinkId, string? messageHint, CancellationToken cancellationToken = default) =>
            await Task.FromResult($"{messageType}|{deploymentId}|{contextId}|{resourceLinkId}|{messageHint}");

        public async Task<(string MessageType, string DeploymentId, string? ContextId, string? ResourceLinkId, string? MessageHint)> ParseLtiMessageHintAsync(string messageHint, CancellationToken cancellationToken = default) =>
            await Task.FromResult(messageHint.Split('|', 5) is [var messageType, var deploymentId, var contextId, var resourceLinkId, var messageHintString] ?
                (messageType, deploymentId, contextId, resourceLinkId, messageHintString) :
                (string.Empty, string.Empty, null, null, null));
    }
}
