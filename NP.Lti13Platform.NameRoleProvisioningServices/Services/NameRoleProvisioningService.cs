using Microsoft.Extensions.Options;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Services
{
    internal class NameRoleProvisioningService(IOptionsMonitor<ServicesConfig> config) : INameRoleProvisioningService
    {
        public Task<ServicesConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default) => Task.FromResult(config.CurrentValue);
    }
}
