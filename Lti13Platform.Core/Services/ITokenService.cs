using NP.Lti13Platform.Core.Configs;

namespace NP.Lti13Platform.Core.Services
{
    public interface ITokenService
    {
        Task<Lti13PlatformTokenConfig> GetTokenConfigAsync(string clientId, CancellationToken cancellationToken = default);
    }
}
