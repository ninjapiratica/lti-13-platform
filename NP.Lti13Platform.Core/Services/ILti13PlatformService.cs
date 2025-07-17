using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services
{
    /// <summary>
    /// Defines the contract for a service that retrieves LTI platform information.
    /// </summary>
    public interface ILti13PlatformService
    {
        /// <summary>
        /// Asynchronously retrieves platform details based on the client ID.
        /// </summary>
        /// <param name="clientId">The client ID of the platform.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Platform"/> if found; otherwise, null.</returns>
        Task<Platform?> GetPlatformAsync(string clientId, CancellationToken cancellationToken = default);
    }
}
