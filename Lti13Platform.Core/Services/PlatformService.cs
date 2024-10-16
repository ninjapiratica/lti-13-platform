using Microsoft.Extensions.Options;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services
{
    internal class PlatformService(IOptionsMonitor<Platform> config) : IPlatformService
    {
        public virtual async Task<Platform?> GetPlatformAsync(string clientId) => await Task.FromResult(config.CurrentValue);
    }
}
