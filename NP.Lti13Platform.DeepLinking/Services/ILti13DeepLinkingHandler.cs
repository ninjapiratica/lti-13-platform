using Microsoft.AspNetCore.Http;

namespace NP.Lti13Platform.DeepLinking.Services
{
    public interface ILti13DeepLinkingHandler
    {
        Task<IResult> HandleResponseAsync(string clientId, string deploymentId, string? contextId, DeepLinkResponse response, CancellationToken cancellationToken = default);
    }
}
