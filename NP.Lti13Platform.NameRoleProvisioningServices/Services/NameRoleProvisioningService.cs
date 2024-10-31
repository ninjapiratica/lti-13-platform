using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Services
{
    internal class NameRoleProvisioningService(IOptionsMonitor<ServicesConfig> config, IHttpContextAccessor httpContextAccessor) : ILti13NameRoleProvisioningService
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
