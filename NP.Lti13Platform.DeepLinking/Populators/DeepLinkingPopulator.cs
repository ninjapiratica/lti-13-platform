﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Populators;
using NP.Lti13Platform.DeepLinking.Services;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.DeepLinking.Populators;

public interface IDeepLinkingMessage : ILaunchPresentationMessage
{
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/version")]
    string LtiVersion { get; set; }

    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/deployment_id")]
    string DeploymentId { get; set; }

    [JsonPropertyName("https://purl.imsglobal.org/spec/lti-dl/claim/deep_linking_settings")]
    DeepLinkSettingsMessage DeepLinkSettings { get; set; }

    public class DeepLinkSettingsMessage
    {
        [JsonPropertyName("deep_link_return_url")]
        public required string DeepLinkReturnUrl { get; set; }

        [JsonPropertyName("accept_types")]
        public required IEnumerable<string> AcceptTypes { get; set; }

        [JsonPropertyName("accept_presentation_document_targets")]
        public required IEnumerable<string> AcceptPresentationDocumentTargets { get; set; }

        [JsonPropertyName("accept_media_types")]
        public string? AcceptMediaTypesSerialized => AcceptMediaTypes == null ? null : string.Join(",", AcceptMediaTypes);

        [JsonIgnore]
        public IEnumerable<string>? AcceptMediaTypes { get; set; }

        [JsonPropertyName("accept_multiple")]
        public bool? AcceptMultiple { get; set; }

        [JsonPropertyName("accept_lineitem")]
        public bool? AcceptLineItem { get; set; }

        [JsonPropertyName("auto_create")]
        public bool? AutoCreate { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }
    }
}

public class DeepLinkingPopulator(LinkGenerator linkGenerator, ILti13DeepLinkingConfigService deepLinkingService) : Populator<IDeepLinkingMessage>
{
    public override async Task PopulateAsync(IDeepLinkingMessage obj, MessageScope scope, CancellationToken cancellationToken = default)
    {
        obj.LtiVersion = "1.3.0";
        obj.DeploymentId = scope.Deployment.Id;

        DeepLinkSettingsOverride? deepLinkSettings = default;
        LaunchPresentationOverride? launchPresentation = default;

        if (!string.IsNullOrWhiteSpace(scope.MessageHint))
        {
            var parts = Encoding.UTF8.GetString(Convert.FromBase64String(scope.MessageHint)).Split('|');
            deepLinkSettings = JsonSerializer.Deserialize<DeepLinkSettingsOverride>(parts[0]);
            launchPresentation = JsonSerializer.Deserialize<LaunchPresentationOverride>(parts[1]);
        }

        var config = await deepLinkingService.GetConfigAsync(scope.Tool.ClientId, cancellationToken);

        obj.DeepLinkSettings = new IDeepLinkingMessage.DeepLinkSettingsMessage
        {
            AcceptPresentationDocumentTargets = deepLinkSettings?.AcceptPresentationDocumentTargets ?? config.AcceptPresentationDocumentTargets,
            AcceptTypes = deepLinkSettings?.AcceptTypes ?? config.AcceptTypes,
            DeepLinkReturnUrl = linkGenerator.GetUriByName(RouteNames.DEEP_LINKING_RESPONSE, new { contextId = scope.Context?.Id }, config.ServiceAddress.Scheme, new HostString(config.ServiceAddress.Authority)) ?? string.Empty,
            AcceptLineItem = deepLinkSettings?.AcceptLineItem ?? config.AcceptLineItem,
            AcceptMediaTypes = deepLinkSettings?.AcceptMediaTypes ?? config.AcceptMediaTypes,
            AcceptMultiple = deepLinkSettings?.AcceptMultiple ?? config.AcceptMultiple,
            AutoCreate = deepLinkSettings?.AutoCreate ?? config.AutoCreate,
            Data = deepLinkSettings?.Data,
            Text = deepLinkSettings?.Text,
            Title = deepLinkSettings?.Title,
        };

        if (launchPresentation != null)
        {
            obj.LaunchPresentation = new ILaunchPresentationMessage.LaunchPresentationDefinition
            {
                DocumentTarget = launchPresentation.DocumentTarget,
                Height = launchPresentation.Height,
                Locale = launchPresentation.Locale,
                ReturnUrl = launchPresentation.ReturnUrl,
                Width = launchPresentation.Width,
            };
        }
    }
}
