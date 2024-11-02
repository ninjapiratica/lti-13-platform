using NP.Lti13Platform.Core.Configs;

namespace NP.Lti13Platform.Core.Services
{
    public interface ILti13TokenConfigService
    {
        Task<Lti13PlatformTokenConfig> GetTokenConfigAsync(string clientId, CancellationToken cancellationToken = default);
    }
}
