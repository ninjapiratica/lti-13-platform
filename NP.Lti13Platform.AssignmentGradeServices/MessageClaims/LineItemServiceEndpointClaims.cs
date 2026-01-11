using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.AssignmentGradeServices.Configs;
using NP.Lti13Platform.AssignmentGradeServices.Constants;
using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core.MessageHandlers;
using NP.Lti13Platform.Core.Models;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.AssignmentGradeServices.MessageClaims;

/// <summary>
/// Represents the service endpoints for assignment grade services.
/// </summary>
public interface ILineItemServiceEndpointClaims : ILtiResourceLinkRequestMessage
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
/// Provides extension methods for adding line item service endpoint claims to objects that support line item service endpoint claims.
/// </summary>
/// <remarks>This class contains static methods that assist in populating service endpoint claims based on tool configuration, deployment, context, and line item information.
/// These methods are intended to be used with types that implement the ILineItemServiceEndpointClaims interface.</remarks>
public static partial class ClaimsExtensions
{
    private static readonly ISet<string> LineItemServiceScopes = new HashSet<string>
    {
        ServiceScopes.LineItem,
        ServiceScopes.LineItemReadOnly,
        ServiceScopes.ResultReadOnly,
        ServiceScopes.Score
    };

    /// <summary>
    /// Adds line item service endpoint claims to the specified object based on the provided tool, deployment, context, and line item information.
    /// </summary>
    /// <remarks>This method populates the <c>ServiceEndpoints</c> property of the object if the tool contains any relevant line item service scopes. 
    /// The generated endpoints reflect the deployment, context, and optionally the line item specified.</remarks>
    /// <typeparam name="T">The type of the object to which line item service endpoint claims are added. Must implement <see cref="ILineItemServiceEndpointClaims"/>.</typeparam>
    /// <param name="obj">The object to which the line item service endpoint claims will be added.</param>
    /// <param name="tool">The tool containing the available service scopes used to determine which endpoints to include.</param>
    /// <param name="deploymentId">The deployment identifier associated with the service endpoints.</param>
    /// <param name="contextId">The context identifier associated with the service endpoints.</param>
    /// <param name="lineItemId">The line item identifier to include in the claims, or <see langword="null"/> to omit the line item endpoint.</param>
    /// <param name="config">The configuration settings used to generate service endpoint URLs.</param>
    /// <param name="linkGenerator">The link generator used to create URLs for the service endpoints.</param>
    /// <returns>The original object with its service endpoint claims populated according to the provided parameters.</returns>
    public static T WithLineItemServiceEndpointClaims<T>(
        this T obj,
        Tool tool,
        DeploymentId deploymentId,
        ContextId contextId,
        LineItemId? lineItemId,
        ServicesConfig config,
        LinkGenerator linkGenerator)
        where T : ILineItemServiceEndpointClaims
    {
        var lineItemScopes = tool.ServiceScopes
            .Intersect(LineItemServiceScopes)
            .ToList();

        if (lineItemScopes.Count > 0)
        {
            obj.ServiceEndpoints = new ILineItemServiceEndpointClaims.LineItemServiceEndpoints
            {
                Scopes = lineItemScopes,
                LineItemsUrl = linkGenerator.GetUriByName(
                    RouteNames.GET_LINE_ITEMS,
                    new
                    {
                        DeploymentId = deploymentId,
                        ContextId = contextId
                    },
                    config.ServiceAddress.Scheme,
                    new HostString(config.ServiceAddress.Authority)),
                LineItemUrl = lineItemId == null || lineItemId == LineItemId.Empty
                    ? null
                    : linkGenerator.GetUriByName(
                        RouteNames.GET_LINE_ITEM,
                        new
                        {
                            DeploymentId = deploymentId,
                            ContextId = contextId,
                            LineItemId = lineItemId
                        },
                        config.ServiceAddress.Scheme,
                        new HostString(config.ServiceAddress.Authority)),
            };
        }

        return obj;
    }
}

internal class LineItemServiceMessageExtension(
    IAssignmentGradeDataService dataService,
    IAssignmentGradeConfigService configService,
    LinkGenerator linkGenerator)
    : ILtiResourceLinkMessageExtension<ILineItemServiceEndpointClaims>
{
    public async Task ExtendMessageAsync(ILineItemServiceEndpointClaims message, Tool tool, ResourceLink resourceLink, User? user, CancellationToken cancellationToken = default)
    {
        var config = await configService.GetConfigAsync(tool.ClientId, cancellationToken);
        var lineItems = await dataService.GetLineItemsAsync(resourceLink.DeploymentId, resourceLink.ContextId, pageIndex: 0, limit: 1, cancellationToken: cancellationToken);

        LineItemId? lineItemId = null;
        if (lineItems.TotalItems == 1)
        {
            lineItemId = lineItems.Items.First().Id;
        }

        message.WithLineItemServiceEndpointClaims(
            tool,
            resourceLink.DeploymentId,
            resourceLink.ContextId,
            lineItemId,
            config,
            linkGenerator);
    }
}