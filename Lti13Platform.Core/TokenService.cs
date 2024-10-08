using Microsoft.Extensions.Options;

namespace NP.Lti13Platform.Core
{
    public interface ITokenService
    {
        Task<Lti13PlatformTokenConfig> GetTokenConfigAsync(string clientId);
    }

    internal class TokenService(IOptionsMonitor<Lti13PlatformCoreConfig> config) : ITokenService
    {
        public virtual async Task<Lti13PlatformTokenConfig> GetTokenConfigAsync(string clientId) => await Task.FromResult(config.CurrentValue.TokenConfig);
    }
}
