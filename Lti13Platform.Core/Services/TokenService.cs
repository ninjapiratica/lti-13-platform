using Microsoft.Extensions.Options;
using NP.Lti13Platform.Core.Configs;

namespace NP.Lti13Platform.Core.Services
{
    internal class TokenService(IOptionsMonitor<Lti13PlatformTokenConfig> config) : ITokenService
    {
        public virtual async Task<Lti13PlatformTokenConfig> GetTokenConfigAsync(string clientId) => await Task.FromResult(config.CurrentValue);
    }
}
