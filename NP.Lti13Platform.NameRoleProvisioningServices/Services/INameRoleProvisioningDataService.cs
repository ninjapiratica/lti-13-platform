using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Services;

/// <summary>
/// Defines a service for accessing name and role provisioning data in an LTI 1.3 platform.
/// </summary>
public interface INameRoleProvisioningDataService
{
    /// <summary>
    /// Retrieves a collection of memberships based on the specified deployment, context, role, resource link, and optional date.
    /// </summary>
    /// <remarks>If no filters are provided, all memberships for the specified deployment and context are retrieved.
    /// The operation supports cancellation via the <paramref name="cancellationToken"/>.</remarks>
    /// <param name="deploymentId">The identifier of the deployment for which memberships are being retrieved.</param>
    /// <param name="contextId">The identifier of the context within the deployment.</param>
    /// <param name="role">An optional role filter. If specified, only memberships matching the role will be included.</param>
    /// <param name="resourceLinkId">An optional resource link identifier. If specified, only memberships associated with the resource link will be included.</param>
    /// <param name="asOfDate">An optional date to retrieve memberships as they existed at a specific point in time. If null, the current memberships are retrieved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains an enumerable collection of <see cref="Membership"/> objects that match the specified criteria.</returns>
    Task<IEnumerable<Membership>> GetMembershipsAsync(DeploymentId deploymentId, ContextId contextId, string? role, ResourceLinkId? resourceLinkId, DateTime? asOfDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a collection of users based on the specified user IDs.
    /// </summary>
    /// <param name="userIds">A collection of user IDs for which to retrieve user information. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IEnumerable{T}"/> of
    /// <see cref="User"/> objects corresponding to the specified user IDs. If no users are found for the provided IDs,
    /// the result will be an empty collection.</returns>
    Task<IEnumerable<User>> GetUsersAsync(IEnumerable<UserId> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the permissions for a specified set of users within a given deployment and optional context.
    /// </summary>
    /// <remarks>This method is designed to handle multiple users in a single call, optimizing performance for batch operations.
    /// Ensure that the <paramref name="userIds"/> parameter contains valid user identifiers to avoid unnecessary processing.</remarks>
    /// <param name="deploymentId">The identifier of the deployment for which to retrieve user permissions.</param>
    /// <param name="contextId">An optional identifier for the context within the deployment. If null, permissions are retrieved for the entire deployment.</param>
    /// <param name="userIds">A collection of user identifiers for whom permissions will be retrieved. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains an enumerable collection of <see cref="UserPermissions"/> objects, each representing the permissions for a specific user.
    /// The collection will be empty if no permissions are found for the specified users.</returns>
    Task<IEnumerable<UserPermissions>> GetUserPermissionsAsync(DeploymentId deploymentId, ContextId? contextId, IEnumerable<UserId> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the permissions for a specified set of users within a given deployment and optional context.
    /// </summary>
    /// <remarks>This method is designed to handle multiple users in a single call, optimizing performance for batch operations.
    /// Ensure that the <paramref name="userIds"/> parameter contains valid user identifiers to avoid unnecessary processing.</remarks>
    /// <param name="deploymentId">The identifier of the deployment for which to retrieve user permissions.</param>
    /// <param name="contextId">An optional identifier for the context within the deployment. If null, permissions are retrieved for the entire deployment.</param>
    /// <param name="userIds">A collection of user identifiers for whom permissions will be retrieved. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains an enumerable collection of <see cref="UserPermissions"/> objects, each representing the permissions for a specific user.
    /// The collection will be empty if no permissions are found for the specified users.</returns>
    Task<IEnumerable<CustomPermissions>> GetCustomPermissionsAsync(DeploymentId deploymentId, ContextId? contextId, IEnumerable<UserId> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the collection of attempts for the specified resource link and users.
    /// </summary>
    /// <param name="resourceLinkId">The identifier of the resource link for which to retrieve attempts.</param>
    /// <param name="userIds">A collection of user identifiers specifying the users whose attempts are to be retrieved. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see cref="Attempt"/> objects corresponding to the specified users and resource link.
    /// The collection is empty if no attempts are found.</returns>
    Task<IEnumerable<Attempt?>> GetAttemptsAsync(ResourceLinkId resourceLinkId, IEnumerable<UserId> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the grades for the specified users associated with a given line item.
    /// </summary>
    /// <param name="lineItemId">The identifier of the line item for which to retrieve grades.</param>
    /// <param name="userIds">A collection of user identifiers specifying the users whose grades are to be retrieved. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of grades for the specified users. The collection is empty if no grades are found.</returns>
    Task<IEnumerable<Grade?>> GetGradesAsync(LineItemId lineItemId, IEnumerable<UserId> userIds, CancellationToken cancellationToken = default);

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
    /// Gets a resource link by its ID.
    /// </summary>
    /// <param name="resourceLinkId">The resource link ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resource link.</returns>
    Task<ResourceLink?> GetResourceLinkAsync(ResourceLinkId resourceLinkId, CancellationToken cancellationToken = default);
}