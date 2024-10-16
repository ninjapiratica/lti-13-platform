using Microsoft.AspNetCore.Http;
using NP.Lti13Platform.DeepLinking.Configs;

namespace NP.Lti13Platform.DeepLinking.Services
{
    public interface IServiceHelper
    {
        Task<IResult> HandleResponseAsync(DeepLinkResponse response);

        Task<DeepLinkingConfig> GetConfigAsync(string clientId);
    }
}
