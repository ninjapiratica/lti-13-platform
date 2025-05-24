namespace NP.Lti13Platform.Core.Configs;

public class Lti13PlatformCoreEndpointsConfig
{
    /// <summary>
    /// Endpoint for the authorization of LTI 1.3 requests.
    /// </summary>
    /// <value>Default: /lti13/authorization</value>
    public string AuthorizationUrl { get; set; } = "/lti13/authorization";

    /// <summary>
    /// Endpoint for getting a set of public JWKs.
    /// </summary>
    /// <value>Default: /lti13/jwks</value>
    public string JwksUrl { get; set; } = "/lti13/jwks";

    /// <summary>
    /// Endpoint used to get auth tokens used for service calls.
    /// </summary>
    /// <value>Default: /lti13/token</value>
    public string TokenUrl { get; set; } = "/lti13/token";
}