namespace NP.Lti13Platform.Core
{
    public class Lti13PlatformConfig
    {
        private const string INVALID_ISSUER = "Issuer must follow the guidelines in the LTI 1.3 security spec. https://www.imsglobal.org/spec/security/v1p0/#dfn-issuer-identifier";
        private const string HTTPS = "https";

        private string _issuer = string.Empty;
        public string Issuer
        {
            get => _issuer;
            set
            {
                if (Uri.TryCreate(value, UriKind.Absolute, out var result))
                {
                    if (result.Scheme == HTTPS && string.IsNullOrWhiteSpace(result.Query) && string.IsNullOrWhiteSpace(result.Fragment))
                    {
                        _issuer = value;
                        return;
                    }
                }

                throw new UriFormatException(INVALID_ISSUER);
            }
        }

        public int IdTokenExpirationMinutes { get; set; } = 5;
    }
}
