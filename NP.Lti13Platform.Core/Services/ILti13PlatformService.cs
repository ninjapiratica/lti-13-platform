using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services
{
    public interface ILti13PlatformService
    {
        Task<Platform?> GetPlatformAsync(string clientId, CancellationToken cancellationToken = default);
    }
}
