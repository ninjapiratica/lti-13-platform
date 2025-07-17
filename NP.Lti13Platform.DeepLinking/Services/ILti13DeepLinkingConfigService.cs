using NP.Lti13Platform.DeepLinking.Configs;

namespace NP.Lti13Platform.DeepLinking.Services
{
    public interface ILti13DeepLinkingConfigService
    {
        Task<DeepLinkingConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default);
    }
}
