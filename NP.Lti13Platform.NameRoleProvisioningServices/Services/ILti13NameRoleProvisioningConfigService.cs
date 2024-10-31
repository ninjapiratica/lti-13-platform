using NP.Lti13Platform.NameRoleProvisioningServices.Configs;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Services
{
    public interface ILti13NameRoleProvisioningConfigService
    {
        Task<ServicesConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default);
    }
}
