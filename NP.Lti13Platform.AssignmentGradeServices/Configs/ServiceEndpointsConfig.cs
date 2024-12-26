namespace NP.Lti13Platform.AssignmentGradeServices.Configs;

public class ServiceEndpointsConfig
{
    /// <summary>
    /// Endpoint used to get a list of line items or create a new line item.
    /// <para>Must include route parameters for {deploymentId} and {contextId}.</para>
    /// </summary>
    /// <value>Default: /lti13/{deploymentId}/{contextId}/lineItems</value>
    public string LineItemsUrl { get; set; } = "/lti13/{deploymentId}/{contextId}/lineItems";

    /// <summary>
    /// Endpoint used to Get/Update/Delete a line item. Also used as the base url for getting results or posting scores.
    /// <para>Must include route parameters for {deploymentId}, {contextId} and {lineItemId}.</para>
    /// </summary>
    /// <value>Default:/lti13/{deploymentId}/{contextId}/lineItems/{lineItemId}</value>
    public string LineItemUrl { get; set; } = "/lti13/{deploymentId}/{contextId}/lineItems/{lineItemId}";
}
