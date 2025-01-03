﻿using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using System.Text;
using System.Text.Json;

namespace NP.Lti13Platform.DeepLinking
{
    public static class ServiceExtensions
    {
        public static async Task<Uri> GetDeepLinkInitiationUrlAsync(
            this IUrlServiceHelper service,
            Tool tool,
            string deploymentId,
            string userId,
            bool isAnonymous,
            string? actualUserId = null,
            string? contextId = null,
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
}
