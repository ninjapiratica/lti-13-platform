using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Populators;
using NP.Lti13Platform.DeepLinking.Services;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.DeepLinking.Populators;

/// <summary>
/// Defines the contract for a deep linking message in LTI 1.3.
/// </summary>
public interface IDeepLinkingMessage : ILaunchPresentationMessage
{
    /// <summary>
    /// Gets or sets the LTI version used for the deep linking.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/version")]
    string LtiVersion { get; set; }

    /// <summary>
    /// Gets or sets the deployment identifier.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/deployment_id")]
    DeploymentId DeploymentId { get; set; }

    /// <summary>
    /// Gets or sets the deep linking settings.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti-dl/claim/deep_linking_settings")]
    DeepLinkSettingsMessage DeepLinkSettings { get; set; }

    /// <summary>
    /// Represents the settings for a deep linking operation.
    /// </summary>
    public class DeepLinkSettingsMessage
    {
        /// <summary>
        /// Gets or sets the URL to return to after deep linking completes.
        /// </summary>
        [JsonPropertyName("deep_link_return_url")]
        public required string DeepLinkReturnUrl { get; set; }

        /// <summary>
        /// Gets or sets the content types that are acceptable for this deep linking request.
        /// </summary>
        [JsonPropertyName("accept_types")]
        public required IEnumerable<string> AcceptTypes { get; set; }

        /// <summary>
        /// Gets or sets the presentation document targets that are acceptable for this deep linking request.
        /// </summary>
        [JsonPropertyName("accept_presentation_document_targets")]
        public required IEnumerable<string> AcceptPresentationDocumentTargets { get; set; }

        /// <summary>
        /// Gets the serialized form of acceptable media types as a comma-separated string.
        /// </summary>
        [JsonPropertyName("accept_media_types")]
        public string? AcceptMediaTypesSerialized => AcceptMediaTypes == null ? null : string.Join(",", AcceptMediaTypes);

        /// <summary>
        /// Gets or sets the media types that are acceptable for this deep linking request.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<string>? AcceptMediaTypes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether multiple content items can be selected.
        /// </summary>
        [JsonPropertyName("accept_multiple")]
        public bool? AcceptMultiple { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether line items can be accepted.
        /// </summary>
        [JsonPropertyName("accept_lineitem")]
        public bool? AcceptLineItem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether content items should be automatically created.
        /// </summary>
        [JsonPropertyName("auto_create")]
        public bool? AutoCreate { get; set; }

        /// <summary>
        /// Gets or sets the title for the deep linking request.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the descriptive text for the deep linking request.
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets opaque data that will be returned with the content item(s).
        /// </summary>
        [JsonPropertyName("data")]
        public string? Data { get; set; }
    }
}

/// <summary>
/// Populates deep linking message properties for LTI 1.3 platform integration.
/// </summary>
/// <param name="linkGenerator">The link generator used to create URLs.</param>
/// <param name="deepLinkingService">The service that provides deep linking configuration.</param>
public class DeepLinkingPopulator(LinkGenerator linkGenerator, ILti13DeepLinkingConfigService deepLinkingService) : Populator<IDeepLinkingMessage>
{
    /// <summary>
    /// Populates a deep linking message with the appropriate values based on the message scope.
    /// </summary>
    /// <param name="obj">The deep linking message to populate.</param>
    /// <param name="scope">The message scope containing context information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous population operation.</returns>
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
