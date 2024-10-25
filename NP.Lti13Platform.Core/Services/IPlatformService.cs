using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services
{
    public interface IPlatformService
    {
        Task<Platform?> GetPlatformAsync(string clientId, CancellationToken cancellationToken = default);
    }
}
