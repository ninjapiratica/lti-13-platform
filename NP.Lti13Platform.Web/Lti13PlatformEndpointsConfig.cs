namespace NP.Lti13Platform.Web
{
    public class Lti13PlatformEndpointsConfig
    {
        /// <summary>
        /// Endpoint for the authorization of LTI 1.3 requests.
        /// </summary>
        /// <value>Default: /lti13/authorization</value>
        public string AuthorizationUrl { get; set; } = "/lti13/authorization";
    }
}