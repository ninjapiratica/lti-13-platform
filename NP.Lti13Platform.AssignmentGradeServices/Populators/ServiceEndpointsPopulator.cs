using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core.Populators;
using NP.Lti13Platform.Core.Services;
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

    public class ServiceEndpointsPopulator(LinkGenerator linkGenerator, ICoreDataService dataService, IAssignmentGradeService assignmentGradeService) : Populator<IServiceEndpoints>
    {
        public override async Task PopulateAsync(IServiceEndpoints obj, MessageScope scope, CancellationToken cancellationToken = default)
        {
            var lineItemScopes = scope.Tool.ServiceScopes
                .Intersect([ServiceScopes.LineItem, ServiceScopes.LineItemReadOnly, ServiceScopes.ResultReadOnly, ServiceScopes.Score])
                .ToList();

            if (lineItemScopes.Count > 0 && scope.Context != null)
            {
                string? lineItemId = null;

                if (scope.ResourceLink != null)
                {
                    var lineItems = await dataService.GetLineItemsAsync(scope.Deployment.Id, scope.Context.Id, 0, 1, null, scope.ResourceLink.Id, null, cancellationToken);
                    if (lineItems.TotalItems == 1)
                    {
                        lineItemId = lineItems.Items.FirstOrDefault()?.Id;
                    }
                }

                var config = await assignmentGradeService.GetConfigAsync(scope.Tool.ClientId, cancellationToken);

                obj.ServiceEndpoints = new IServiceEndpoints.LineItemServiceEndpoints
                {
                    Scopes = lineItemScopes,
                    LineItemsUrl = linkGenerator.GetUriByName(RouteNames.GET_LINE_ITEMS, new { deploymentId = scope.Deployment.Id, contextId = scope.Context.Id }, config.ServiceAddress.Scheme, new HostString(config.ServiceAddress.Authority)),
                    LineItemUrl = string.IsNullOrWhiteSpace(lineItemId) ? null : linkGenerator.GetUriByName(RouteNames.GET_LINE_ITEM, new { deploymentId = scope.Deployment.Id, contextId = scope.Context.Id, lineItemId }, config.ServiceAddress.Scheme, new HostString(config.ServiceAddress.Authority)),
                };
            }
        }
    }
}
