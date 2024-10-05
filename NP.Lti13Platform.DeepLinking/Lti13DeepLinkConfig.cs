using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.DeepLinking
{
    public partial class Lti13DeepLinkingConfig
    {
        private const string MEDIA_TYPE_IMAGE = "image/*";
        private const string MEDIA_TYPE_TEXT_HTML = "text/html";

        public IEnumerable<string> AcceptPresentationDocumentTargets { get; set; } = [Lti13PresentationTargetDocuments.Embed, Lti13PresentationTargetDocuments.Iframe, Lti13PresentationTargetDocuments.Window];
        public IEnumerable<string> AcceptTypes { get; set; } = [Lti13DeepLinkingTypes.File, Lti13DeepLinkingTypes.Html, Lti13DeepLinkingTypes.Image, Lti13DeepLinkingTypes.Link, Lti13DeepLinkingTypes.LtiResourceLink];
        public IEnumerable<string> AcceptMediaTypes { get; set; } = [MEDIA_TYPE_IMAGE, MEDIA_TYPE_TEXT_HTML];

        public bool? AcceptLineItem { get; set; } = true;
        public bool? AcceptMultiple { get; set; } = true;
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
