using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services;

/// <summary>
/// Defines the contract for a service that handles LTI 1.3 core data operations.
/// </summary>
public interface ILtiResourceLinkMessageDataService
{
    /// <summary>
    /// Gets a deployment by its ID.
    /// </summary>
    /// <param name="deploymentId">The deployment ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deployment.</returns>
    Task<Deployment?> GetDeploymentAsync(DeploymentId deploymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a context by its ID.
    /// </summary>
    /// <param name="contextId">The context ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The context.</returns>
    Task<Context?> GetContextAsync(ContextId contextId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user.</returns>
    Task<User?> GetUserAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a membership by context and user IDs.
    /// </summary>
    /// <param name="contextId">The context ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The membership.</returns>
    Task<Membership?> GetMembershipAsync(ContextId contextId, UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a resource link by its ID.
    /// </summary>
    /// <param name="resourceLinkId">The resource link ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resource link.</returns>
    Task<ResourceLink?> GetResourceLinkAsync(ResourceLinkId resourceLinkId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of line items.
    /// </summary>
    /// <param name="resourceLinkId">The resource link ID.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="limit">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A partial list of line items.</returns>
    Task<PartialList<LineItem>> GetLineItemsAsync(ResourceLinkId resourceLinkId, int pageIndex, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an attempt by resource link and user IDs.
    /// </summary>
    /// <param name="resourceLinkId">The resource link ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The attempt.</returns>
    Task<Attempt?> GetAttemptAsync(ResourceLinkId resourceLinkId, UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a grade by line item and user IDs.
    /// </summary>
    /// <param name="lineItemId">The line item ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The grade.</returns>
    Task<Grade?> GetGradeAsync(LineItemId lineItemId, UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the custom permissions.
    /// </summary>
    /// <param name="deploymentId">The deployment ID.</param>
    /// <param name="contextId">The context ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="actualUserId">The actual user ID (if impersonating).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The custom permissions.</returns>
    Task<CustomPermissions> GetCustomPermissionsAsync(DeploymentId deploymentId, ContextId? contextId, UserId? userId, UserId? actualUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user permissions.
    /// </summary>
    /// <param name="deploymentId">The deployment ID.</param>
    /// <param name="contextId">The context ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user permissions.</returns>
    Task<UserPermissions> GetUserPermissionsAsync(DeploymentId deploymentId, ContextId? contextId, UserId userId, CancellationToken cancellationToken = default);
}