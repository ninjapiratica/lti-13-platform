using Microsoft.AspNetCore.Http;
using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Populators;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.AssignmentGradeServices.Populators
{
    public interface IServiceEndpoints
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti-ags/claim/endpoint")]
        public LineItemServiceEndpoints? ServiceEndpoints { get; set; }

        public class LineItemServiceEndpoints
        {
            [JsonPropertyName("scope")]
            public required IEnumerable<string> Scopes { get; set; }

            [JsonPropertyName("lineitems")]
            public string? LineItemsUrl { get; set; }

            [JsonPropertyName("lineitem")]
            public string? LineItemUrl { get; set; }
        }
    }

    public class ServiceEndpointsPopulator(IHttpContextAccessor httpContextAccessor, ILtiLinkGenerator linkGenerator, ICoreDataService dataService, IServiceHelper assignmentGradeService) : Populator<IServiceEndpoints>
    {
        public override async Task PopulateAsync(IServiceEndpoints obj, Lti13MessageScope scope)
        {
            var httpContext = httpContextAccessor.HttpContext;

            var lineItemScopes = scope.Tool.ServiceScopes
                .Intersect([ServiceScopes.LineItem, ServiceScopes.LineItemReadOnly, ServiceScopes.ResultReadOnly, ServiceScopes.Score])
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

                var config = await assignmentGradeService.GetConfigAsync(scope.Tool.ClientId);

                obj.ServiceEndpoints = new IServiceEndpoints.LineItemServiceEndpoints
                {
                    Scopes = lineItemScopes,
                    LineItemsUrl = linkGenerator.GetUriString(RouteNames.GET_LINE_ITEMS, new { deploymentId = scope.Deployment.Id, contextId = scope.Context.Id }, httpContext.Request, config.ServiceAddress),
                    LineItemUrl = string.IsNullOrWhiteSpace(lineItemId) ? null : linkGenerator.GetUriString(RouteNames.GET_LINE_ITEM, new { deploymentId = scope.Deployment.Id, contextId = scope.Context.Id, lineItemId }, httpContext.Request, config.ServiceAddress),
                };
            }
        }
    }
}
