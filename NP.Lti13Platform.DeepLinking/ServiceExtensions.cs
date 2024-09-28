using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using System.Text;
using System.Text.Json;

namespace NP.Lti13Platform.DeepLinking
{
    public static class ServiceExtensions
    {
        // TODO: figure out launch_presentation
        public static Uri GetDeepLinkInitiationUrl(this Service service, Tool tool, string deploymentId, string? contextId, string? userId = null, DeepLinkSettingsOverride? deepLinkSettings = null)
            => service.GetUrl(Lti13MessageType.LtiDeepLinkingRequest, tool, deploymentId, tool.DeepLinkUrl, contextId, null, userId, Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(deepLinkSettings))));
    }
}
