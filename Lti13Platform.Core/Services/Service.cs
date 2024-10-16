using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Populators;
using System.Text;
using System.Text.Json;
using System.Web;

namespace NP.Lti13Platform.Core.Services
{
    public class Service(ITokenService tokenService)
    {
        public async Task<Uri> GetResourceLinkInitiationUrlAsync(Tool tool, string deploymentId, string contextId, ResourceLink resourceLink, string userId, bool isAnonymous, string? actualUserId = null, LaunchPresentationOverride? launchPresentation = null)
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
                Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(launchPresentation))));

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
            string? messageHint = null)
        {
            var builder = new UriBuilder(tool.OidcInitiationUrl);

            var query = HttpUtility.ParseQueryString(builder.Query);
            query.Add("iss", (await tokenService.GetTokenConfigAsync(tool.ClientId)).Issuer);
            query.Add("login_hint", $"{userId}|{(isAnonymous ? "1" : string.Empty)}|{actualUserId}");
            query.Add("target_link_uri", targetLinkUri);
            query.Add("client_id", tool.ClientId.ToString());
            query.Add("lti_message_hint", $"{messageType}|{deploymentId}|{contextId}|{resourceLinkId}|{messageHint}");
            query.Add("lti_deployment_id", deploymentId);
            builder.Query = query.ToString();

            return builder.Uri;
        }
    }
}
