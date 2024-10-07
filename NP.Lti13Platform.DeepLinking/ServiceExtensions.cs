using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using System.Text;
using System.Text.Json;

namespace NP.Lti13Platform.DeepLinking
{
    public static class ServiceExtensions
    {
        public static Uri GetDeepLinkInitiationUrl(
            this Service service,
            Tool tool,
            string deploymentId,
            string? contextId = null,
            string? userId = null,
            string? actualUserId = null,
            DeepLinkSettingsOverride? deepLinkSettings = null,
            LaunchPresentationOverride? launchPresentation = null)
            => service.GetUrl(
                Lti13MessageType.LtiDeepLinkingRequest,
                tool,
                deploymentId,
                tool.DeepLinkUrl,
                contextId,
                resourceLinkId: null,
                userId,
                actualUserId,
                Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(deepLinkSettings) + "|" + JsonSerializer.Serialize(launchPresentation))));
    }
}
