using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.Core;

namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    public class PopulateServiceEndpoints(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator) : Populator<IServiceEndpoints>
    {
        public override async Task Populate(IServiceEndpoints obj, Lti13MessageScope scope)
        {
            if (scope.Tool.ServicePermissions.AllowNameRoleProvisioningService && !string.IsNullOrWhiteSpace(scope.Context?.Id) && httpContextAccessor.HttpContext != null)
            {
                obj.NamesRoleService = new IServiceEndpoints.ServiceEndpoints
                {
                    ContextMembershipsUrl = linkGenerator.GetUriByName(httpContextAccessor.HttpContext, RouteNames.GET_MEMBERSHIPS, new { contextId = scope.Context.Id })!,
                    ServiceVersions = ["2.0"]
                };
            }

            await Task.CompletedTask;
        }
    }
}
