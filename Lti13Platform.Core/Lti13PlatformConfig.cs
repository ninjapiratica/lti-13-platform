namespace NP.Lti13Platform
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

        private string _tokenAudience = string.Empty;
        public string TokenAudience
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

        public Lti13PlatformClaim? PlatformClaim { get; set; }

        public class Lti13DeepLinkConfig
        {
            public IEnumerable<string> AcceptPresentationDocumentTargets { get; set; } = [Lti13PresentationTargetDocuments.Embed, Lti13PresentationTargetDocuments.Iframe, Lti13PresentationTargetDocuments.Window];
            public IEnumerable<string> AcceptTypes { get; set; } = [Lti13DeepLinkingTypes.File, Lti13DeepLinkingTypes.Html, Lti13DeepLinkingTypes.Image, Lti13DeepLinkingTypes.Link, Lti13DeepLinkingTypes.LtiResourceLink];
            public IEnumerable<string> AcceptMediaTypes { get; set; } = [MEDIA_TYPE_IMAGE, MEDIA_TYPE_TEXT_HTML];

            public bool? AcceptLineItem { get; set; }
            public bool? AcceptMultiple { get; set; }
            public bool? AutoCreate { get; set; }

            // this property is auto-set in the UseLtiPlatform() startup method
            public string ReturnUrl { get; set; } = string.Empty;
        }
    }
}
