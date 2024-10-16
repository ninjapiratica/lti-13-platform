namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    public class Lti13PlatformNameRoleProvisioningServicesEndpointsConfig
    {
        /// <summary>
        /// Endpoint used to get a list of members in the context.
        /// <para>Must include route parameters for {deploymentId} and {contextId}.</para>
        /// </summary>
        /// <value>Default: /lti13/{deploymentId}/{contextId}/memberships</value>
        public string NamesAndRoleProvisioningServicesUrl { get; set; } = "/lti13/{deploymentId}/{contextId}/memberships";
    }
}
