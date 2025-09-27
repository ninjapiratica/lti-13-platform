using Microsoft.Extensions.Options;
using NP.Lti13Platform.Core.Configs;

namespace NP.Lti13Platform.Core.Services;

internal class DefaultTokenConfigService(IOptionsMonitor<Lti13PlatformTokenConfig> config) : ILti13TokenConfigService
{
    public async Task<Lti13PlatformTokenConfig> GetTokenConfigAsync(string toolId, CancellationToken cancellationToken = default) => await Task.FromResult(config.CurrentValue);
}
