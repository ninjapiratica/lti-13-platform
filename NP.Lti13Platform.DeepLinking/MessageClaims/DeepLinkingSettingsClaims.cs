using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.DeepLinking.Configs;
using NP.Lti13Platform.DeepLinking.Constants;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.DeepLinking.MessageClaims;

/// <summary>
/// Defines the contract for a deep linking message in LTI 1.3.
/// </summary>
public interface IDeepLinkingSettingsClaims
{
    /// <summary>
    /// Gets or sets the deep linking settings.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti-dl/claim/deep_linking_settings")]
    DeepLinkingSettingsClaim DeepLinkSettings { get; set; }

    /// <summary>
    /// Represents the settings for a deep linking operation.
    /// </summary>
    public class DeepLinkingSettingsClaim
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
/// Provides extension methods for populating claims related to deep linking settings on objects that support deep linking claims.
/// </summary>
/// <remarks>These extension methods assist in setting deep linking configuration claims by combining provided overrides with default configuration values.
/// They are intended for use with types that implement the IDeepLinkingSettingsClaims interface.</remarks>
public static partial class ClaimsExtensions
{
    /// <summary>
    /// Populates the deep linking settings claims on the specified object using the provided context, settings override, configuration, and link generator.
    /// </summary>
    /// <remarks>If a value in deepLinkSettings is null, the corresponding value from config is used. The generated deep link return URL is based on the provided context and configuration.</remarks>
    /// <typeparam name="T">The type of the object to update, which must implement IDeepLinkingSettingsClaims.</typeparam>
    /// <param name="obj">The object whose deep linking settings claims will be set.</param>
    /// <param name="contextId">The context identifier used to generate the deep link return URL.</param>
    /// <param name="deepLinkingSettings">An optional override for deep linking settings. If null, values from the configuration are used.</param>
    /// <param name="config">The configuration containing default deep linking settings and service address information.</param>
    /// <param name="linkGenerator">The link generator used to create the deep link return URL.</param>
    /// <returns>The same object instance with its deep linking settings claims populated.</returns>
    public static T WithDeepLinkingSettingsClaims<T>(
        this T obj,
        DeepLinkingConfig config,
        LinkGenerator linkGenerator,
        ContextId? contextId,
        DeepLinkingSettingsOverride? deepLinkingSettings)
        where T : IDeepLinkingSettingsClaims
    {
        obj.DeepLinkSettings = new IDeepLinkingSettingsClaims.DeepLinkingSettingsClaim
        {
            AcceptPresentationDocumentTargets = deepLinkingSettings?.AcceptPresentationDocumentTargets ?? config.AcceptPresentationDocumentTargets,
            AcceptTypes = deepLinkingSettings?.AcceptTypes ?? config.AcceptTypes,
            DeepLinkReturnUrl = linkGenerator.GetUriByName(
                RouteNames.DEEP_LINKING_RESPONSE,
                new
                {
                    ContextId = contextId
                },
                config.ServiceAddress.Scheme,
                new HostString(config.ServiceAddress.Authority)) ?? string.Empty,
            AcceptLineItem = deepLinkingSettings?.AcceptLineItem ?? config.AcceptLineItem,
            AcceptMediaTypes = deepLinkingSettings?.AcceptMediaTypes ?? config.AcceptMediaTypes,
            AcceptMultiple = deepLinkingSettings?.AcceptMultiple ?? config.AcceptMultiple,
            AutoCreate = deepLinkingSettings?.AutoCreate ?? config.AutoCreate,
            Data = deepLinkingSettings?.Data,
            Text = deepLinkingSettings?.Text,
            Title = deepLinkingSettings?.Title,
        };

        return obj;
    }
}
