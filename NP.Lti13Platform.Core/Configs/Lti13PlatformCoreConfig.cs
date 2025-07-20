namespace NP.Lti13Platform.Core.Configs;

/// <summary>
/// Configuration for LTI 1.3 platform tokens.
/// </summary>
public class Lti13PlatformTokenConfig
{
    private string _issuer = string.Empty;
    /// <summary>
    /// A case-sensitive URL using the HTTPS scheme that contains: scheme, host; and, optionally, port number, and path components; and, no query or fragment components. The issuer identifies the platform to the tools.
    /// </summary>
    public required string Issuer
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

            throw new UriFormatException("Issuer must follow the guidelines in the LTI 1.3 security spec. https://www.imsglobal.org/spec/security/v1p0/#dfn-issuer-identifier");
        }
    }

    /// <summary>
    /// The value used to validate a token request from a tool. This is used to compare against the 'aud' claim of that JWT token request.
    /// </summary>
    public string? TokenAudience { get; set; }

    /// <summary>
    /// The expiration time in seconds for message tokens.
    /// </summary>
    public int MessageTokenExpirationSeconds { get; set; } = 300;

    /// <summary>
    /// The expiration time in seconds for access tokens.
    /// </summary>
    public int AccessTokenExpirationSeconds { get; set; } = 3600;
}