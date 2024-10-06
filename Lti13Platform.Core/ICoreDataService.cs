using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core
{
    public interface ICoreDataService
    {
        Task<Tool?> GetToolAsync(string clientId);
        Task<Deployment?> GetDeploymentAsync(string deploymentId);
        Task<Context?> GetContextAsync(string contextId);
        Task<User?> GetUserAsync(string userId);
        Task<Membership?> GetMembershipAsync(string contextId, string userId);
        Task<IEnumerable<string>> GetMentoredUserIdsAsync(string contextId, string userId);
        Task<ResourceLink?> GetResourceLinkAsync(string resourceLinkId);

        Task<PartialList<LineItem>> GetLineItemsAsync(string deploymentId, string contextId, int pageIndex, int limit, string? resourceId = null, string? resourceLinkId = null, string? tag = null);

        Task<Attempt?> GetAttemptAsync(string resourceLinkId, string userId);

        Task<Grade?> GetGradeAsync(string lineItemId, string userId);

        Task<ServiceToken?> GetServiceTokenRequestAsync(string toolId, string id);
        Task SaveServiceTokenRequestAsync(ServiceToken serviceToken);

        Task<IEnumerable<SecurityKey>> GetPublicKeysAsync();
        Task<SecurityKey> GetPrivateKeyAsync();
    }
}
