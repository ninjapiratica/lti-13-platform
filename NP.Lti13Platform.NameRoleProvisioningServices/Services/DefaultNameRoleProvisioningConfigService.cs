using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Services
{
    internal class DefaultNameRoleProvisioningConfigService(IOptionsMonitor<ServicesConfig> config, IHttpContextAccessor httpContextAccessor) : ILti13NameRoleProvisioningConfigService
    {
        public async Task<ServicesConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default)
        {
            var servicesConfig = config.CurrentValue;
            if (servicesConfig.ServiceAddress == ServicesConfig.DefaultUri)
            {
                servicesConfig = servicesConfig with { ServiceAddress = new UriBuilder(httpContextAccessor.HttpContext?.Request.Scheme, httpContextAccessor.HttpContext?.Request.Host.Value).Uri };
            }

            return await Task.FromResult(servicesConfig);
        }
    }
}
