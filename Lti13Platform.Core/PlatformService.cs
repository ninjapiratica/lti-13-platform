using Microsoft.Extensions.Options;

namespace NP.Lti13Platform
{
    internal class PlatformService(IOptionsMonitor<Lti13PlatformConfig> config) : IPlatformService
    {
        public async Task<Platform?> GetPlatformAsync(string clientId) => await Task.FromResult(config.CurrentValue.Platform);
    }
}
