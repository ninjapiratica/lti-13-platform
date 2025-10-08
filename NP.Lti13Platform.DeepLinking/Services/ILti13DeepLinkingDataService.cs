using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.DeepLinking.Models;

namespace NP.Lti13Platform.DeepLinking.Services;

/// <summary>
/// Defines a service for managing deep linking data in an LTI 1.3 platform.
/// </summary>
public interface ILti13DeepLinkingDataService
{
    /// <summary>
    /// Saves a content item to the data store.
    /// </summary>
    /// <param name="deploymentId">The deployment identifier.</param>
    /// <param name="contextId">Optional. The context identifier.</param>
    /// <param name="contentItem">The content item to save.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveContentItemAsync(DeploymentId deploymentId, ContextId? contextId, ContentItem contentItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a content item to the data store.
    /// </summary>
    /// <param name="deploymentId">The deployment identifier.</param>
    /// <param name="contextId">Optional. The context identifier.</param>
    /// <param name="resourceLinkContentItem">The resource link content item to save.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identifier of the saved resource link.</returns>
    Task<ResourceLinkId> SaveResourceLinkAsync(DeploymentId deploymentId, ContextId? contextId, LtiResourceLinkContentItem resourceLinkContentItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a line item to the data store.
    /// </summary>
    /// <param name="lineItem">The line item to save.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identifier of the saved line item.</returns>
    Task<LineItemId> SaveLineItemAsync(LineItem lineItem, CancellationToken cancellationToken = default);
}
