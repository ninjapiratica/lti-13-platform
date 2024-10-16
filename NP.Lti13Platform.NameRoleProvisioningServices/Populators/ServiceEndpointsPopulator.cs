using Microsoft.AspNetCore.Http;
using NP.Lti13Platform.Core.Populators;
using NP.Lti13Platform.Core.Services;
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

    public class ServiceEndpointsPopulator(IHttpContextAccessor httpContextAccessor, LtiLinkGenerator linkGenerator, IServiceHelper nameRoleProvisioningService) : Populator<IServiceEndpoints>
    {
        public override async Task PopulateAsync(IServiceEndpoints obj, MessageScope scope)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (scope.Tool.ServiceScopes.Contains(Lti13ServiceScopes.MembershipReadOnly) && !string.IsNullOrWhiteSpace(scope.Context?.Id) && httpContext != null)
            {
                var config = await nameRoleProvisioningService.GetConfigAsync(scope.Tool.ClientId);

                obj.NamesRoleService = new IServiceEndpoints.ServiceEndpoints
                {
                    ContextMembershipsUrl = linkGenerator.GetUriString(RouteNames.GET_MEMBERSHIPS, new { deploymentId = scope.Deployment.Id, contextId = scope.Context.Id }, httpContext.Request, config.ServiceAddress),
                    ServiceVersions = ["2.0"]
                };
            }
        }
    }
}
