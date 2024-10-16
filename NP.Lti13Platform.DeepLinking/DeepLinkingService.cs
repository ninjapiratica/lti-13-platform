using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace NP.Lti13Platform.DeepLinking
{
    public interface IDeepLinkingService
    {
        Task<IResult> HandleResponseAsync(DeepLinkResponse response);

        Task<Lti13DeepLinkingConfig> GetConfigAsync(string clientId);
    }

    internal class DeepLinkingService(IOptionsMonitor<Lti13DeepLinkingConfig> config) : IDeepLinkingService
    {
        public virtual Task<IResult> HandleResponseAsync(DeepLinkResponse response)
        {
            return Task.FromResult(Results.Ok(response));
        }

        public virtual Task<Lti13DeepLinkingConfig> GetConfigAsync(string clientId) => Task.FromResult(config.CurrentValue);
    }
}
