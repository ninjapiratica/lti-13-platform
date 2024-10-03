using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core
{
    public class Lti13PlatformConfig
    {
        private const string INVALID_ISSUER = "Issuer must follow the guidelines in the LTI 1.3 security spec. https://www.imsglobal.org/spec/security/v1p0/#dfn-issuer-identifier";
        private const string INVALID_TOKEN_AUDIENCE = "Token Audience must follow the guidelines in the LTI 1.3 security spec. https://www.imsglobal.org/spec/security/v1p0/#dfn-issuer-identifier";
        private const string MEDIA_TYPE_IMAGE = "image/*";
        private const string MEDIA_TYPE_TEXT_HTML = "text/html";

        private string _issuer = string.Empty;
        public string Issuer
        {
            get => _issuer;
            set
            {
                if (Uri.TryCreate(value, UriKind.Absolute, out var result))
                {
                    if (result.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase) && string.IsNullOrWhiteSpace(result.Query) && string.IsNullOrWhiteSpace(result.Fragment))
                    {
                        _issuer = value;
                        return;
                    }
                }

                throw new UriFormatException(INVALID_ISSUER);
            }
        }

        private string? _tokenAudience;
        public string? TokenAudience
        {
            get => _tokenAudience;
            set
            {
                if (Uri.TryCreate(value, UriKind.Absolute, out var result))
                {
                    if (result.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase) && string.IsNullOrWhiteSpace(result.Query) && string.IsNullOrWhiteSpace(result.Fragment))
                    {
                        _tokenAudience = value;
                        return;
                    }
                }

                throw new UriFormatException(INVALID_TOKEN_AUDIENCE);
            }
        }

        public Lti13DeepLinkConfig DeepLink { get; set; } = new Lti13DeepLinkConfig();

        public int IdTokenExpirationSeconds { get; set; } = 300;

        public int AccessTokenExpirationSeconds { get; set; } = 3600;

        public Platform? Platform { get; set; }

        public IDictionary<(string? ToolId, string ContentItemType), Type> ContentItemTypes { get; set; } = new ContentItemDictionary();

        public void AddDefaultContentItemMapping()
        {
            ContentItemTypes.Add((null, ContentItemType.File), typeof(FileContentItem));
            ContentItemTypes.Add((null, ContentItemType.Html), typeof(HtmlContentItem));
            ContentItemTypes.Add((null, ContentItemType.Image), typeof(ImageContentItem));
            ContentItemTypes.Add((null, ContentItemType.Link), typeof(LinkContentItem));
            ContentItemTypes.Add((null, ContentItemType.LtiResourceLink), typeof(LtiResourceLinkContentItem));
        }

        public class Lti13DeepLinkConfig
        {
            public IEnumerable<string> AcceptPresentationDocumentTargets { get; set; } = [Lti13PresentationTargetDocuments.Embed, Lti13PresentationTargetDocuments.Iframe, Lti13PresentationTargetDocuments.Window];
            public IEnumerable<string> AcceptTypes { get; set; } = [Lti13DeepLinkingTypes.File, Lti13DeepLinkingTypes.Html, Lti13DeepLinkingTypes.Image, Lti13DeepLinkingTypes.Link, Lti13DeepLinkingTypes.LtiResourceLink];
            public IEnumerable<string> AcceptMediaTypes { get; set; } = [MEDIA_TYPE_IMAGE, MEDIA_TYPE_TEXT_HTML];

            public bool? AcceptLineItem { get; set; } = true;
            public bool? AcceptMultiple { get; set; } = true;
            public bool? AutoCreate { get; set; } = true;
        }
    }
}
