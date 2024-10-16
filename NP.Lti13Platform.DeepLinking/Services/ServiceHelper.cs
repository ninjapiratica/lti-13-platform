using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NP.Lti13Platform.DeepLinking.Configs;

namespace NP.Lti13Platform.DeepLinking.Services
{
    internal class ServiceHelper(IOptionsMonitor<DeepLinkingConfig> config) : IServiceHelper
    {
        public virtual Task<IResult> HandleResponseAsync(DeepLinkResponse response)
        {
            return Task.FromResult(Results.Ok(response));
        }

        public virtual Task<DeepLinkingConfig> GetConfigAsync(string clientId) => Task.FromResult(config.CurrentValue);
    }
}
