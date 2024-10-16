namespace NP.Lti13Platform.DeepLinking
{
    public class Lti13DeepLinkingEndpointsConfig
    {
        /// <summary>
        /// Endpoint for the response of LTI 1.3 deep link messages.
        /// <para>Must include route parameter for {contextId?}.</para>
        /// </summary>
        /// <value>Default: /lti13/deeplinking/{contextId?}</value>
        public string DeepLinkingResponseUrl { get; set; } = "/lti13/deeplinking/{contextId?}";
    }
}
