using Microsoft.Extensions.Options;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services
{
    internal class PlatformService(IOptionsMonitor<Platform> config) : ILti13PlatformService
    {
        public async Task<Platform?> GetPlatformAsync(string clientId, CancellationToken cancellationToken = default) => await Task.FromResult(!string.IsNullOrWhiteSpace(config.CurrentValue.Guid) ? config.CurrentValue : null);
    }
}
