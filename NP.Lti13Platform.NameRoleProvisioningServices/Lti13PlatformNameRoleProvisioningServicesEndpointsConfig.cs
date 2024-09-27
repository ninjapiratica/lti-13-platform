namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    public class Lti13PlatformNameRoleProvisioningServicesEndpointsConfig
    {
        /// <summary>
        /// Endpoint used to get a list of members in the context.
        /// <para>Must include route parameter for {contextId}.</para>
        /// </summary>
        /// <value>Default: /lti13/{contextId}/memberships</value>
        public string NamesAndRoleProvisioningServiceUrl { get; set; } = "/lti13/{contextId}/memberships";
    }
}
