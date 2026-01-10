using Microsoft.Extensions.Options;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services;

/// <summary>
/// Defines the contract for a service that retrieves LTI platform information.
/// </summary>
public interface IPlatformService
{
    /// <summary>
    /// Asynchronously retrieves platform details based on the tool identifier.
    /// </summary>
    /// <param name="clientId">The tool identifier of the platform.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Platform"/> if found; otherwise, null.</returns>
    Task<Platform?> GetPlatformAsync(ClientId clientId, CancellationToken cancellationToken = default);
}

internal class DefaultPlatformService(IOptionsMonitor<Platform> config) : IPlatformService
{
    public async Task<Platform?> GetPlatformAsync(ClientId clientId, CancellationToken cancellationToken = default) => await Task.FromResult(!string.IsNullOrWhiteSpace(config.CurrentValue.Guid) ? config.CurrentValue : null);
}