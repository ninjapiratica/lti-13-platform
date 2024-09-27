namespace NP.Lti13Platform.AssignmentGradeServices
{
    public class Lti13PlatformAGSEndpointsConfig
    {
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
    }
}
