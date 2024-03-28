namespace NP.Lti13Platform
{
    public class Lti13PlatformEndpointsConfig
    {
        /// <summary>
        /// Endpoint for the authorization of LTI 1.3 requests.
        /// Default value is "/lti13/authorization".
        /// </summary>
        /// <value>Default: /lti13/authorization</value>
        public string AuthorizationUrl { get; set; } = "/lti13/authorization";

        /// <summary>
        /// Endpoint for the response of LTI 1.3 deep link messages.
        /// Default value is "/lti13/deeplink".
        /// </summary>
        public string DeepLinkResponseUrl { get; set; } = "/lti13/deeplink";

        /// <summary>
        /// Endpoint for getting a set of public JWKs.
        /// Default value is "/lti13/jwks".
        /// </summary>
        public string JwksUrl { get; set; } = "/lti13/jwks";

        /// <summary>
        /// Endpoint used to get a list of line items or create a new line item.
        /// Must include route parameter for {contextId}.
        /// Default value is "/lti13/{contextId}/lineItems".
        /// </summary>
        public string AssignmentAndGradeServiceLineItemsUrl { get; set; } = "/lti13/{contextId}/lineItems";

        /// <summary>
        /// Endpoint used to Get/Update/Delete a line item. Also used as the base url for getting results or posting scores.
        /// Must include route parameters for {contextId} and {lineItemId}.
        /// Default value is "/lti13/{contextId}/lineItems/{lineItemId}".
        /// </summary>
        public string AssignmentAndGradeServiceLineItemBaseUrl { get; set; } = "/lti13/{contextId}/lineItems/{lineItemId}";

        /// <summary>
        /// Endpoint used to get auth tokens used for service calls.
        /// </summary>
        public string TokenUrl { get; set; } = "/lti13/token";
    }
}