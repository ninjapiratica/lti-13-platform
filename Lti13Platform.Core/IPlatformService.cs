using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core
{
    public interface IPlatformService
    {
        Task<Platform?> GetPlatformAsync(string clientId);
    }
}
