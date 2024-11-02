using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NP.Lti13Platform.DeepLinking.Configs;

namespace NP.Lti13Platform.DeepLinking.Services
{
    internal class DefaultDeepLinkingConfigService(IOptionsMonitor<DeepLinkingConfig> config, IHttpContextAccessor httpContextAccessor) : ILti13DeepLinkingConfigService
    {
        public async Task<DeepLinkingConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default)
        {
            var deepLinkingConfig = config.CurrentValue;
            if (deepLinkingConfig.ServiceAddress == DeepLinkingConfig.DefaultUri)
            {
                deepLinkingConfig = deepLinkingConfig with { ServiceAddress = new UriBuilder(httpContextAccessor.HttpContext?.Request.Scheme, httpContextAccessor.HttpContext?.Request.Host.Value).Uri };
            }

            return await Task.FromResult(deepLinkingConfig);
        }
    }
}
