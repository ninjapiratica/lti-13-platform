namespace NP.Lti13Platform.Core
{
    public class Lti13PlatformCoreConfig
    {
        private const string INVALID_DEEP_LINK_RETURN_URL = "DeepLinkReturnUrl must follow the guidelines in the LTI spec for urls. https://www.imsglobal.org/spec/lti/v1p3/#messages-and-services";
        private const string INVALID_ISSUER = "Issuer must follow the guidelines in the LTI 1.3 security spec. https://www.imsglobal.org/spec/security/v1p0/#dfn-issuer-identifier";
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

        public Lti13DeepLinkConfig DeepLink { get; set; } = new Lti13DeepLinkConfig();

        public void CopyTo(Lti13PlatformCoreConfig config)
        {
            config._issuer = _issuer;
            DeepLink.CopyTo(config.DeepLink);
        }

        public class Lti13DeepLinkConfig
        {
            private string _deepLinkReturnUrl = string.Empty;
            public string DeepLinkReturnUrl
            {
                get => _deepLinkReturnUrl;
                set
                {
                    if (Uri.TryCreate(value, UriKind.Absolute, out var result))
                    {
                        if (result.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase))
                        {
                            _deepLinkReturnUrl = value;
                            return;
                        }
                    }

                    throw new UriFormatException(INVALID_DEEP_LINK_RETURN_URL);
                }
            }

            public IEnumerable<string> AcceptPresentationDocumentTargets { get; set; } = [Lti13PresentationTargetDocuments.Embed, Lti13PresentationTargetDocuments.Iframe, Lti13PresentationTargetDocuments.Window];
            public IEnumerable<string> AcceptTypes { get; set; } = [Lti13DeepLinkingTypes.File, Lti13DeepLinkingTypes.Html, Lti13DeepLinkingTypes.Image, Lti13DeepLinkingTypes.Link, Lti13DeepLinkingTypes.LtiResourceLink];
            public IEnumerable<string> AcceptMediaTypes { get; set; } = [MEDIA_TYPE_IMAGE, MEDIA_TYPE_TEXT_HTML];

            public bool AcceptLineItem { get; set; }
            public bool AcceptMultiple { get; set; }
            public bool AutoCreate { get; set; }

            public void CopyTo(Lti13DeepLinkConfig config)
            {
                config._deepLinkReturnUrl = _deepLinkReturnUrl;
                config.AcceptPresentationDocumentTargets = AcceptPresentationDocumentTargets.ToList();
                config.AcceptTypes = AcceptTypes.ToList();
                config.AcceptMediaTypes = AcceptMediaTypes.ToList();
                config.AcceptLineItem = AcceptLineItem;
                config.AcceptMultiple = AcceptMultiple;
                config.AutoCreate = AutoCreate;
            }
        }
    }
}
