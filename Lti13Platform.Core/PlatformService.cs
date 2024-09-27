using Microsoft.Extensions.Options;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core
{
    internal class PlatformService(IOptionsMonitor<Lti13PlatformConfig> config) : IPlatformService
    {
        public async Task<Platform?> GetPlatformAsync(string clientId) => await Task.FromResult(config.CurrentValue.Platform);
    }
}
