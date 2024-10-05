using Microsoft.Extensions.Options;
using NP.Lti13Platform.Core.Models;
using System.Text;
using System.Text.Json;
using System.Web;

namespace NP.Lti13Platform.Core
{
    public class Service(IOptionsMonitor<Lti13PlatformConfig> config)
    {
        public Uri GetResourceLinkInitiationUrl(Tool tool, string deploymentId, string contextId, LtiResourceLinkContentItem resourceLink, string? userId = null, LaunchPresentationOverride? launchPresentation = null)
            => GetUrl(
                Lti13MessageType.LtiResourceLinkRequest,
                tool,
                deploymentId,
                string.IsNullOrWhiteSpace(resourceLink.Url) ? tool.LaunchUrl : resourceLink.Url,
                contextId,
                resourceLink.Id,
                userId,
                Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(launchPresentation))));

        public Uri GetUrl(string messageType, Tool tool, string deploymentId, string targetLinkUri, string? contextId = null, string? resourceLinkId = null, string? userId = null, string? messageHint = null)
        {
            var builder = new UriBuilder(tool.OidcInitiationUrl);

            var query = HttpUtility.ParseQueryString(builder.Query);
            query.Add("iss", config.CurrentValue.Issuer);
            query.Add("login_hint", userId);
            query.Add("target_link_uri", targetLinkUri);
            query.Add("client_id", tool.ClientId.ToString());
            query.Add("lti_message_hint", $"{messageType}|{deploymentId}|{contextId}|{resourceLinkId}|{messageHint}");
            query.Add("lti_deployment_id", deploymentId);
            builder.Query = query.ToString();

            return builder.Uri;
        }
    }
}
