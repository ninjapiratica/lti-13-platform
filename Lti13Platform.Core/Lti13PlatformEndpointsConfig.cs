namespace NP.Lti13Platform
{
    public class Lti13PlatformEndpointsConfig
    {
        /// <summary>
        /// Endpoint for the authorization of LTI 1.3 requests.
        /// </summary>
        /// <value>Default: /lti13/authorization</value>
        public string AuthorizationUrl { get; set; } = "/lti13/authorization";

        /// <summary>
        /// Endpoint for the response of LTI 1.3 deep link messages.
        /// </summary>
        public string DeepLinkResponseUrl { get; set; } = "/lti13/deeplink";

        /// <summary>
        /// 
        /// </summary>
        public string JwksUrl { get; set; } = "/lti13/jwks";
    }
}