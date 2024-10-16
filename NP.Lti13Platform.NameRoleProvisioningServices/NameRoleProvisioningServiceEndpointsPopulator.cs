using Microsoft.AspNetCore.Http;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Populators;

namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    public class NameRoleProvisioningServiceEndpointsPopulator(IHttpContextAccessor httpContextAccessor, ILtiLinkGenerator linkGenerator, INameRoleProvisioningService nameRoleProvisioningService) : Populator<IServiceEndpoints>
    {
        public override async Task PopulateAsync(IServiceEndpoints obj, Lti13MessageScope scope)
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
