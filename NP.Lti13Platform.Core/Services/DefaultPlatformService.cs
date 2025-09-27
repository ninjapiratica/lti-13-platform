using Microsoft.Extensions.Options;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services;

internal class DefaultPlatformService(IOptionsMonitor<Platform> config) : ILti13PlatformService
{
    public async Task<Platform?> GetPlatformAsync(string toolId, CancellationToken cancellationToken = default) => await Task.FromResult(!string.IsNullOrWhiteSpace(config.CurrentValue.Guid) ? config.CurrentValue : null);
}
