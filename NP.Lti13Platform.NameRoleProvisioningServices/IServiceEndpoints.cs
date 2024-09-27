using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    public interface IServiceEndpoints
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti-nrps/claim/namesroleservice")]
        public ServiceEndpoints? NamesRoleService { get; set; }

        public class ServiceEndpoints
        {
            [JsonPropertyName("context_memberships_url")]
            public required string ContextMembershipsUrl { get; set; }

            [JsonPropertyName("service_versions")]
            public required IEnumerable<string> ServiceVersions { get; set; }
        }
    }
}
