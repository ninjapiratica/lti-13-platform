using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.Core.Populators;
using NP.Lti13Platform.NameRoleProvisioningServices.Services;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Populators
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

    public class ServiceEndpointsPopulator(LinkGenerator linkGenerator, INameRoleProvisioningService nameRoleProvisioningService) : Populator<IServiceEndpoints>
    {
        public override async Task PopulateAsync(IServiceEndpoints obj, MessageScope scope, CancellationToken cancellationToken = default)
        {
            if (scope.Tool.ServiceScopes.Contains(Lti13ServiceScopes.MembershipReadOnly) && !string.IsNullOrWhiteSpace(scope.Context?.Id))
            {
                var config = await nameRoleProvisioningService.GetConfigAsync(scope.Tool.ClientId, cancellationToken);

                obj.NamesRoleService = new IServiceEndpoints.ServiceEndpoints
                {
                    ContextMembershipsUrl = linkGenerator.GetUriByName(RouteNames.GET_MEMBERSHIPS, new { deploymentId = scope.Deployment.Id, contextId = scope.Context.Id }, config.ServiceAddress.Scheme, new HostString(config.ServiceAddress.Authority)) ?? string.Empty,
                    ServiceVersions = ["2.0"]
                };
            }
        }
    }
}
