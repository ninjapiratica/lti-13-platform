using Microsoft.Extensions.Options;

namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    public interface INameRoleProvisioningService
    {
        Task<Lti13NameRoleProvisioningServicesConfig> GetConfigAsync(string clientId);
    }

    internal class NameRoleProvisioningService(IOptionsMonitor<Lti13NameRoleProvisioningServicesConfig> config) : INameRoleProvisioningService
    {
        public virtual Task<Lti13NameRoleProvisioningServicesConfig> GetConfigAsync(string clientId) => Task.FromResult(config.CurrentValue);
    }
}
