﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Populators;

namespace NP.Lti13Platform.AssignmentGradeServices.Populators
{
    public class AssignmentGradeServicesEndpointsPopulator(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator, ICoreDataService dataService) : Populator<IServiceEndpoints>
    {
        public override async Task PopulateAsync(IServiceEndpoints obj, Lti13MessageScope scope)
        {
            var httpContext = httpContextAccessor.HttpContext;

            var lineItemScopes = scope.Tool.ServiceScopes
                .Intersect([Lti13ServiceScopes.LineItem, Lti13ServiceScopes.LineItemReadOnly, Lti13ServiceScopes.ResultReadOnly, Lti13ServiceScopes.Score])
                .ToList();

            if (lineItemScopes.Count > 0 && scope.Context != null && httpContext != null)
            {
                string? lineItemId = null;

                if (scope.ResourceLink != null)
                {
                    var lineItems = await dataService.GetLineItemsAsync(scope.Deployment.Id, scope.Context.Id, 0, 1, null, scope.ResourceLink.Id, null);
                    if (lineItems.TotalItems == 1)
                    {
                        lineItemId = lineItems.Items.FirstOrDefault()?.Id;
                    }
                }

                obj.ServiceEndpoints = new IServiceEndpoints.LineItemServiceEndpoints
                {
                    Scopes = lineItemScopes,
                    LineItemsUrl = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEMS, new { deploymentId = scope.Deployment.Id, contextId = scope.Context.Id }),
                    LineItemUrl = string.IsNullOrWhiteSpace(lineItemId) ? null : linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { deploymentId = scope.Deployment.Id, contextId = scope.Context.Id, lineItemId }),
                };
            }
        }
    }
}
