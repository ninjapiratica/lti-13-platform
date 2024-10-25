using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NP.Lti13Platform.DeepLinking.Configs;

namespace NP.Lti13Platform.DeepLinking.Services
{
    internal class DeepLinkingService(IOptionsMonitor<DeepLinkingConfig> config, IHttpContextAccessor httpContextAccessor) : IDepLinkingService
    {
        public Task<IResult> HandleResponseAsync(DeepLinkResponse response, CancellationToken cancellationToken = default) => Task.FromResult(Results.Ok(response));

        public async Task<DeepLinkingConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default)
        {
            var deepLinkingConfig = config.CurrentValue;
            if (deepLinkingConfig.ServiceAddress == null)
            {
                deepLinkingConfig = deepLinkingConfig with { ServiceAddress = new UriBuilder(httpContextAccessor.HttpContext?.Request.Scheme, httpContextAccessor.HttpContext?.Request.Host.Value).Uri };
            }

            return await Task.FromResult(deepLinkingConfig);
        }
    }
}
