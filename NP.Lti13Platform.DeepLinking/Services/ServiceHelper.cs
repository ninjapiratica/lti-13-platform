using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NP.Lti13Platform.DeepLinking.Configs;

namespace NP.Lti13Platform.DeepLinking.Services
{
    internal class ServiceHelper(IOptionsMonitor<DeepLinkingConfig> config) : IServiceHelper
    {
        public Task<IResult> HandleResponseAsync(DeepLinkResponse response, CancellationToken cancellationToken = default) => Task.FromResult(Results.Ok(response));

        public Task<DeepLinkingConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default) => Task.FromResult(config.CurrentValue);
    }
}
