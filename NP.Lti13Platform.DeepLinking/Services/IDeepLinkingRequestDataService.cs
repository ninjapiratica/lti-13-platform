using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.DeepLinking.Services;

/// <summary>
/// Defines a service for managing deep linking data in an LTI 1.3 platform.
/// </summary>
public interface IDeepLinkingRequestDataService
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