namespace NP.Lti13Platform.Core.Configs;

/// <summary>
/// Configuration for LTI 1.3 platform core endpoints.
/// </summary>
public class Lti13PlatformCoreEndpointsConfig
{
    /// <summary>
    /// Gets or sets the endpoint for the authorization of LTI 1.3 requests.
    /// </summary>
    /// <value>Default: /lti13/authorization</value>
    public string AuthorizationUrl { get; set; } = "/lti13/authorization";

    /// <summary>
    /// Gets or sets the endpoint for getting a set of public JWKs.
    /// </summary>
    /// <para>Must include route parameter for {clientId}.</para>
    /// <value>Default: /lti13/jwks/{clientId}</value>
    public string JwksUrl { get; set; } = "/lti13/jwks/{clientId}";

    /// <summary>
    /// Gets or sets the endpoint used to get auth tokens used for service calls.
    /// </summary>
    /// <value>Default: /lti13/token</value>
    public string TokenUrl { get; set; } = "/lti13/token";
}