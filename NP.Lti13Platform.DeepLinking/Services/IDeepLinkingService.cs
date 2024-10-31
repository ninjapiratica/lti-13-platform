using Microsoft.AspNetCore.Http;
using NP.Lti13Platform.DeepLinking.Configs;

namespace NP.Lti13Platform.DeepLinking.Services
{
    public interface IDeepLinkingService
    {
        Task<IResult> HandleResponseAsync(string clientId, string deploymentId, string? contextId, DeepLinkResponse response, CancellationToken cancellationToken = default);

        Task<DeepLinkingConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default);
    }
}
