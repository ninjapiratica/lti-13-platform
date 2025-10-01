using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services
{
    /// <summary>
    /// Defines the contract for a service that handles LTI 1.3 core data operations.
    /// </summary>
    public interface ILti13CoreDataService
    {
        /// <summary>
        /// Gets a tool by its client ID.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The tool.</returns>
        Task<Tool?> GetToolAsync(ClientId clientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a deployment by its ID.
        /// </summary>
        /// <param name="deploymentId">The deployment ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The deployment.</returns>
        Task<Deployment?> GetDeploymentAsync(string deploymentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a context by its ID.
        /// </summary>
        /// <param name="contextId">The context ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The context.</returns>
        Task<Context?> GetContextAsync(string contextId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The user.</returns>
        Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a membership by context and user IDs.
        /// </summary>
        /// <param name="contextId">The context ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The membership.</returns>
        Task<Membership?> GetMembershipAsync(string contextId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a resource link by its ID.
        /// </summary>
        /// <param name="resourceLinkId">The resource link ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The resource link.</returns>
        Task<ResourceLink?> GetResourceLinkAsync(string resourceLinkId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paginated list of line items.
        /// </summary>
        /// <param name="deploymentId">The deployment ID.</param>
        /// <param name="contextId">The context ID.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="limit">The page size.</param>
        /// <param name="resourceId">The resource ID.</param>
        /// <param name="resourceLinkId">The resource link ID.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A partial list of line items.</returns>
        Task<PartialList<LineItem>> GetLineItemsAsync(string deploymentId, string contextId, int pageIndex, int limit, string? resourceId = null, string? resourceLinkId = null, string? tag = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an attempt by resource link and user IDs.
        /// </summary>
        /// <param name="resourceLinkId">The resource link ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The attempt.</returns>
        Task<Attempt?> GetAttemptAsync(string resourceLinkId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a grade by line item and user IDs.
        /// </summary>
        /// <param name="lineItemId">The line item ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The grade.</returns>
        Task<Grade?> GetGradeAsync(string lineItemId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a service token by tool and token IDs.
        /// </summary>
        /// <param name="clientId">The tool ID.</param>
        /// <param name="id">The token ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The service token.</returns>
        Task<ServiceToken?> GetServiceTokenAsync(ClientId clientId, string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves a service token.
        /// </summary>
        /// <param name="serviceToken">The service token to save.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SaveServiceTokenAsync(ServiceToken serviceToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the public keys.
        /// </summary>
        /// <param name="clientId">The tool ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of public keys.</returns>
        Task<IEnumerable<SecurityKey>> GetPublicKeysAsync(ClientId clientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the private key.
        /// </summary>
        /// <param name="clientId">The tool ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The private key.</returns>
        Task<SecurityKey> GetPrivateKeyAsync(ClientId clientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the custom permissions.
        /// </summary>
        /// <param name="deploymentId">The deployment ID.</param>
        /// <param name="contextId">The context ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="actualUserId">The actual user ID (if impersonating).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The custom permissions.</returns>
        Task<CustomPermissions> GetCustomPermissions(string deploymentId, string? contextId, string userId, string? actualUserId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the user permissions.
        /// </summary>
        /// <param name="deploymentId">The deployment ID.</param>
        /// <param name="contextId">The context ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The user permissions.</returns>
        Task<UserPermissions> GetUserPermissionsAsync(string deploymentId, string? contextId, string userId, CancellationToken cancellationToken = default);
    }
}
