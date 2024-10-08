using Microsoft.Extensions.Options;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core
{
    public interface IPlatformService
    {
        Task<Platform?> GetPlatformAsync(string clientId);
    }

    internal class PlatformService(IOptionsMonitor<Lti13PlatformCoreConfig> config) : IPlatformService
    {
        public virtual async Task<Platform?> GetPlatformAsync(string clientId) => await Task.FromResult(config.CurrentValue.Platform);
    }
}
