using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.Core;
using static NP.Lti13Platform.AssignmentGradeServices.IServiceEndpoints;

namespace NP.Lti13Platform.AssignmentGradeServices
{
    public class AssignmentGradeServiceEndpointsPopulator(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator, IDataService dataService) : Populator<IServiceEndpoints>
    {
        public override async Task Populate(IServiceEndpoints obj, Lti13MessageScope scope)
        {
            if (scope.Tool.ServicePermissions.LineItemScopes.Any() && scope.Context != null && httpContextAccessor.HttpContext != null)
            {
                string? lineItemId = null;

                if (scope.ResourceLink != null)
                {
                    var lineItems = await dataService.GetLineItemsAsync(scope.Context.Id, 0, 2, null, scope.ResourceLink?.Id, null);
                    if (lineItems.TotalItems == 1)
                    {
                        lineItemId = lineItems.Items.FirstOrDefault()?.Id;
                    }
                }

                obj.ServiceEndpoints = new LineItemServiceEndpoints
                {
                    Scopes = scope.Tool.ServicePermissions.LineItemScopes.ToList(),
                    LineItemsUrl = linkGenerator.GetUriByName(httpContextAccessor.HttpContext, RouteNames.GET_LINE_ITEMS, new { contextId = scope.Context.Id }),
                    LineItemUrl = string.IsNullOrWhiteSpace(lineItemId) ? null : linkGenerator.GetUriByName(httpContextAccessor.HttpContext, RouteNames.GET_LINE_ITEM, new { contextId = scope.Context.Id, lineItemId = lineItemId }),
                };
            }
        }
    }
}
