using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services
{
    /// <summary>
    /// Defines the contract for a service that provides token configuration for LTI 1.3 integrations.
    /// </summary>
    public interface ILti13TokenConfigService
    {
        /// <summary>
        /// Gets the token configuration for a specific client.
        /// </summary>
        /// <param name="clientId">The tool identifier for which to retrieve token configuration.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the token configuration.</returns>
        Task<Lti13PlatformTokenConfig> GetTokenConfigAsync(ClientId clientId, CancellationToken cancellationToken = default);
    }
}
