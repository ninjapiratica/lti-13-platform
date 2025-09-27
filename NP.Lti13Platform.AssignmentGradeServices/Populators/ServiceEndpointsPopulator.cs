using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core.Populators;
using NP.Lti13Platform.Core.Services;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.AssignmentGradeServices.Populators;

/// <summary>
/// Represents the service endpoints for assignment grade services.
/// </summary>
public interface IServiceEndpoints
{
    /// <summary>
    /// Gets or sets the line item service endpoints.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti-ags/claim/endpoint")]
    public LineItemServiceEndpoints? ServiceEndpoints { get; set; }

    /// <summary>
    /// Represents the line item service endpoints.
    /// </summary>
    public class LineItemServiceEndpoints
    {
        /// <summary>
        /// Gets or sets the scopes for the line item service.
        /// </summary>
        [JsonPropertyName("scope")]
        public required IEnumerable<string> Scopes { get; set; }

        /// <summary>
        /// Gets or sets the URL for managing line items.
        /// </summary>
        [JsonPropertyName("lineitems")]
        public string? LineItemsUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL for managing a specific line item.
        /// </summary>
        [JsonPropertyName("lineitem")]
        public string? LineItemUrl { get; set; }
    }
}

/// <summary>
/// Populates service endpoints for assignment grade services.
/// </summary>
public class ServiceEndpointsPopulator(LinkGenerator linkGenerator, ILti13CoreDataService dataService, ILti13AssignmentGradeConfigService assignmentGradeService) : Populator<IServiceEndpoints>
{
    /// <summary>
    /// Populates the service endpoints for the given scope.
    /// </summary>
    /// <param name="obj">The service endpoints object to populate.</param>
    /// <param name="scope">The message scope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
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

            var config = await assignmentGradeService.GetConfigAsync(scope.Tool.Id, cancellationToken);

            obj.ServiceEndpoints = new IServiceEndpoints.LineItemServiceEndpoints
            {
                Scopes = lineItemScopes,
                LineItemsUrl = linkGenerator.GetUriByName(RouteNames.GET_LINE_ITEMS, new { deploymentId = scope.Deployment.Id, contextId = scope.Context.Id }, config.ServiceAddress.Scheme, new HostString(config.ServiceAddress.Authority)),
                LineItemUrl = string.IsNullOrWhiteSpace(lineItemId) ? null : linkGenerator.GetUriByName(RouteNames.GET_LINE_ITEM, new { deploymentId = scope.Deployment.Id, contextId = scope.Context.Id, lineItemId }, config.ServiceAddress.Scheme, new HostString(config.ServiceAddress.Authority)),
            };
        }
    }
}
