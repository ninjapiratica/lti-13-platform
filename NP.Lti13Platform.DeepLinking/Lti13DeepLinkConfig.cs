using NP.Lti13Platform.Core;
using NP.Lti13Platform.DeepLinking.Models;

namespace NP.Lti13Platform.DeepLinking
{
    public class Lti13DeepLinkingConfig
    {
        private const string MEDIA_TYPE_IMAGE = "image/*";
        private const string MEDIA_TYPE_TEXT_HTML = "text/html";

        public IEnumerable<string> AcceptPresentationDocumentTargets { get; set; } = [Lti13PresentationTargetDocuments.Embed, Lti13PresentationTargetDocuments.Iframe, Lti13PresentationTargetDocuments.Window];
        public IEnumerable<string> AcceptTypes { get; set; } = [Lti13DeepLinkingTypes.File, Lti13DeepLinkingTypes.Html, Lti13DeepLinkingTypes.Image, Lti13DeepLinkingTypes.Link, Lti13DeepLinkingTypes.LtiResourceLink];
        public IEnumerable<string> AcceptMediaTypes { get; set; } = [MEDIA_TYPE_IMAGE, MEDIA_TYPE_TEXT_HTML];

        /// <summary>
        /// Whether the platform in the context of that deep linking request supports or ignores line items included in LTI Resource Link items. False indicates line items will be ignored. True indicates the platform will create a line item when creating the resource link. If the field is not present, no assumption that can be made about the support of line items.
        /// </summary>
        public bool? AcceptLineItem { get; set; } = true;

        /// <summary>
        /// Whether the platform allows multiple content items to be submitted in a single response.
        /// </summary>
        public bool? AcceptMultiple { get; set; } = true;

        /// <summary>
        /// Whether any content items returned by the tool would be automatically persisted without any option for the user to cancel the operation.
        /// </summary>
        public bool? AutoCreate { get; set; } = true;

        public IDictionary<(string? ToolId, string ContentItemType), Type> ContentItemTypes { get; set; } = new ContentItemDictionary();

        public void AddDefaultContentItemMapping()
        {
            ContentItemTypes.Add((null, ContentItemType.File), typeof(FileContentItem));
            ContentItemTypes.Add((null, ContentItemType.Html), typeof(HtmlContentItem));
            ContentItemTypes.Add((null, ContentItemType.Image), typeof(ImageContentItem));
            ContentItemTypes.Add((null, ContentItemType.Link), typeof(LinkContentItem));
            ContentItemTypes.Add((null, ContentItemType.LtiResourceLink), typeof(LtiResourceLinkContentItem));
        }
    }
}
