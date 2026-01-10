using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services;

/// <summary>
/// Defines the contract for a service that handles LTI 1.3 core data operations.
/// </summary>
public interface ICoreDataService
{
    /// <summary>
    /// Gets a tool by its client ID.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tool.</returns>
    Task<Tool?> GetToolAsync(ClientId clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a service token by tool and token IDs.
    /// </summary>
    /// <param name="clientId">The tool ID.</param>
    /// <param name="id">The token ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The service token.</returns>
    Task<ServiceToken?> GetServiceTokenAsync(ClientId clientId, ServiceTokenId id, CancellationToken cancellationToken = default);

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
}