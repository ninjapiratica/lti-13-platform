using Microsoft.AspNetCore.Http;
using NP.Lti13Platform.DeepLinking.Configs;

namespace NP.Lti13Platform.DeepLinking.Services
{
    public interface IDepLinkingService
    {
        Task<IResult> HandleResponseAsync(DeepLinkResponse response, CancellationToken cancellationToken = default);

        Task<DeepLinkingConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default);
    }
}
