using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.Core;

namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    public class NameRoleProvisioningServiceEndpointsPopulator(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator) : Populator<IServiceEndpoints>
    {
        public override async Task Populate(IServiceEndpoints obj, Lti13MessageScope scope)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (scope.Tool.ServiceScopes.Contains(Lti13ServiceScopes.MembershipReadOnly) && !string.IsNullOrWhiteSpace(scope.Context?.Id) && httpContext != null)
            {
                obj.NamesRoleService = new IServiceEndpoints.ServiceEndpoints
                {
                    ContextMembershipsUrl = linkGenerator.GetUriByName(httpContext, RouteNames.GET_MEMBERSHIPS, new { contextId = scope.Context.Id })!,
                    ServiceVersions = ["2.0"]
                };
            }

            await Task.CompletedTask;
        }
    }
}
