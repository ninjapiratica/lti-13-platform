using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Services;

/// <summary>
/// Defines a service for accessing name and role provisioning data in an LTI 1.3 platform.
/// </summary>
public interface ILti13NameRoleProvisioningDataService
{
    /// <summary>
    /// Gets memberships for a specific context.
    /// </summary>
    /// <param name="deploymentId">The deployment identifier.</param>
    /// <param name="contextId">The context identifier.</param>
    /// <param name="pageIndex">The page index for pagination.</param>
    /// <param name="limit">The maximum number of memberships to return.</param>
    /// <param name="role">Optional. Filter memberships by role.</param>
    /// <param name="resourceLinkId">Optional. Filter memberships by resource link identifier.</param>
    /// <param name="asOfDate">Optional. Return memberships as of a specific date.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a partial list of membership and user tuples.</returns>
    Task<PartialList<(Membership Membership, User User)>> GetMembershipsAsync(DeploymentId deploymentId, ContextId contextId, int pageIndex, int limit, string? role, string? resourceLinkId, DateTime? asOfDate = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets user permissions for a list of users in a context.
    /// </summary>
    /// <param name="deploymentId">The deployment identifier.</param>
    /// <param name="contextId">The context identifier.</param>
    /// <param name="userIds">The user identifiers to get permissions for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user permissions.</returns>
    Task<IEnumerable<UserPermissions>> GetUserPermissionsAsync(DeploymentId deploymentId, ContextId? contextId, IEnumerable<UserId> userIds, CancellationToken cancellationToken = default);
}
