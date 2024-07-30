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
        /// <para>Must include route parameter for {contextId?}.</para>
        /// </summary>
        /// <value>Default: /lti13/deeplinking/{contextId?}</value>
        public string DeepLinkingResponseUrl { get; set; } = "/lti13/deeplinking/{contextId?}";

        /// <summary>
        /// Endpoint for getting a set of public JWKs.
        /// </summary>
        /// <value>Default: /lti13/jwks</value>
        public string JwksUrl { get; set; } = "/lti13/jwks";

        /// <summary>
        /// Endpoint used to get a list of line items or create a new line item.
        /// <para>Must include route parameter for {contextId}.</para>
        /// </summary>
        /// <value>Default: /lti13/{contextId}/lineItems</value>
        public string AssignmentAndGradeServiceLineItemsUrl { get; set; } = "/lti13/{contextId}/lineItems";

        /// <summary>
        /// Endpoint used to Get/Update/Delete a line item. Also used as the base url for getting results or posting scores.
        /// <para>Must include route parameters for {contextId} and {lineItemId}.</para>
        /// </summary>
        /// <value>Default:/lti13/{contextId}/lineItems/{lineItemId}</value>
        public string AssignmentAndGradeServiceLineItemBaseUrl { get; set; } = "/lti13/{contextId}/lineItems/{lineItemId}";

        /// <summary>
        /// Endpoint used to get auth tokens used for service calls.
        /// </summary>
        /// <value>Default: /lti13/token</value>
        public string TokenUrl { get; set; } = "/lti13/token";

        /// <summary>
        /// Endpoint used to get a list of members in the context.
        /// <para>Must include route parameter for {contextId}.</para>
        /// </summary>
        /// <value>Default: /lti13/{contextId}/memberships</value>
        public string NamesAndRoleProvisioningServiceUrl { get; set; } = "/lti13/{contextId}/memberships";
    }
}