using Microsoft.Extensions.Options;
using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services;

internal class DefaultTokenConfigService(IOptionsMonitor<Lti13PlatformTokenConfig> config) : ILti13TokenConfigService
{
    public async Task<Lti13PlatformTokenConfig> GetTokenConfigAsync(ClientId clientId, CancellationToken cancellationToken = default) => await Task.FromResult(config.CurrentValue);
}
