namespace NP.Lti13Platform.Core
{
    public class Lti13PlatformTokenConfig
    {
        private const string INVALID_ISSUER = "Issuer must follow the guidelines in the LTI 1.3 security spec. https://www.imsglobal.org/spec/security/v1p0/#dfn-issuer-identifier";
        private const string INVALID_TOKEN_AUDIENCE = "Token Audience must follow the guidelines in the LTI 1.3 security spec. https://www.imsglobal.org/spec/security/v1p0/#dfn-issuer-identifier";

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

        public int IdTokenExpirationSeconds { get; set; } = 300;

        public int AccessTokenExpirationSeconds { get; set; } = 3600;
    }
}
