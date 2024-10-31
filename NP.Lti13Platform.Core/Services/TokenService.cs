using Microsoft.Extensions.Options;
using NP.Lti13Platform.Core.Configs;

namespace NP.Lti13Platform.Core.Services
{
    internal class TokenService(IOptionsMonitor<Lti13PlatformTokenConfig> config) : ILti13TokenService
    {
        public async Task<Lti13PlatformTokenConfig> GetTokenConfigAsync(string clientId, CancellationToken cancellationToken = default) => await Task.FromResult(config.CurrentValue);
    }
}
