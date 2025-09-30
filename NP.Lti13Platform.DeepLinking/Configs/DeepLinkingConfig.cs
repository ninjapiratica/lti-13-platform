using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.DeepLinking.Models;
using System.Net.Mime;

namespace NP.Lti13Platform.DeepLinking.Configs
{
    /// <summary>
    /// Represents the configuration for deep linking in the LTI 1.3 platform.
    /// </summary>
    public record DeepLinkingConfig
    {
        /// <summary>
        /// Gets or sets the accepted presentation document targets.
        /// </summary>
        public IEnumerable<string> AcceptPresentationDocumentTargets { get; set; } = [Lti13PresentationTargetDocuments.Embed, Lti13PresentationTargetDocuments.Iframe, Lti13PresentationTargetDocuments.Window];

        /// <summary>
        /// Gets or sets the accepted content item types.
        /// </summary>
        public IEnumerable<string> AcceptTypes { get; set; } = [Lti13DeepLinkingTypes.File, Lti13DeepLinkingTypes.Html, Lti13DeepLinkingTypes.Image, Lti13DeepLinkingTypes.Link, Lti13DeepLinkingTypes.LtiResourceLink];

        /// <summary>
        /// Gets or sets the accepted media types.
        /// </summary>
        public IEnumerable<string> AcceptMediaTypes { get; set; } = ["image/*", MediaTypeNames.Text.Html];

        /// <summary>
        /// Gets or sets a value indicating whether line items are accepted.
        /// </summary>
        public bool? AcceptLineItem { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether multiple content items are accepted.
        /// </summary>
        public bool? AcceptMultiple { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether content items are automatically created.
        /// </summary>
        public bool? AutoCreate { get; set; } = true;

        /// <summary>
        /// Gets the mapping of content item types.
        /// </summary>
        public IDictionary<(string? ClientId, string ContentItemType), Type> ContentItemTypes { get; } = new ContentItemDictionary();

        /// <summary>
        /// Gets or sets the service address for deep linking.
        /// </summary>
        public Uri ServiceAddress { get; set; } = DefaultUri;

        /// <summary>
        /// Adds default mappings for content item types.
        /// </summary>
        public void AddDefaultContentItemMapping()
        {
            ContentItemTypes.Add((null, ContentItemType.File), typeof(FileContentItem));
            ContentItemTypes.Add((null, ContentItemType.Html), typeof(HtmlContentItem));
            ContentItemTypes.Add((null, ContentItemType.Image), typeof(ImageContentItem));
            ContentItemTypes.Add((null, ContentItemType.Link), typeof(LinkContentItem));
            ContentItemTypes.Add((null, ContentItemType.LtiResourceLink), typeof(LtiResourceLinkContentItem));
        }

        /// <summary>
        /// The default URI for the service.
        /// </summary>
        internal readonly static Uri DefaultUri = new("x://x.x.x");
    }
}
