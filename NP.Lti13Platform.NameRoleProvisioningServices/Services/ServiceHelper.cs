using Microsoft.Extensions.Options;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Services
{
    internal class ServiceHelper(IOptionsMonitor<ServicesConfig> config) : IServiceHelper
    {
        public virtual Task<ServicesConfig> GetConfigAsync(string clientId) => Task.FromResult(config.CurrentValue);
    }
}
