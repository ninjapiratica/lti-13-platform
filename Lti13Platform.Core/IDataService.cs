using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core
{
    public interface IDataService
    {
        Task<Tool?> GetToolAsync(string clientId);
        Task<Deployment?> GetDeploymentAsync(string clientId, string deploymentId);
        Task<Context?> GetContextAsync(string clientId, string deploymentId, string contextId);
        
        Task<PartialList<Membership>> GetMembershipsAsync(string clientId, string deploymentId, string contextId, int pageIndex, int limit, string? role, string? resourceLinkId, DateTime? asOfDate = null);
        Task<PartialList<User>> GetUsersAsync(string clientId, string deploymentId, string contextId, IEnumerable<string> userIds, DateTime? asOfDate = null);
        Task<PartialList<string>> GetRolesAsync(string clientId, string deployment, string contextId, string userId);
        Task<PartialList<string>> GetMentoredUserIdsAsync(string clientId, string deploymentId, string contextId, string userId);

        Task SaveResourceLinksAsync(string clientId, string deploymentId, string contextId, IEnumerable<LtiResourceLinkContentItem> contentItems);
        Task<LtiResourceLinkContentItem?> GetResourceLinkAsync(string clientId, string deploymentId, string contextId, string contentItemId);

        Task<PartialList<LineItem>> GetLineItemsAsync(string clientId, string deployment, string contextId, int pageIndex, int limit, string? resourceId = null, string? resourceLinkId = null, string? tag = null, string? lineItemId = null);
        Task SaveLineItemAsync(string clientId, string deploymentId, string contextId, LineItem lineItem);
        Task DeleteLineItemAsync(string clientId, string deploymentId, string contextId, string lineItemId);

        Task<Attempt?> GetAttemptAsync(string clientId, string deploymentId, string contextId, string resourceLinkId, string userId);

        Task<PartialList<Grade>> GetGradesAsync(string clientId, string deploymentId, string contextId, string lineItemId, int pageIndex, int limit, string? userId);
        Task SaveGradeAsync(string clientId, string deploymentId, string contextId, Grade result);

        Task<ServiceToken?> GetServiceTokenRequestAsync(string clientId, string deploymentId, string id);
        Task SaveServiceTokenRequestAsync(string clientId, string deploymentId, ServiceToken serviceToken);

        Task<IEnumerable<SecurityKey>> GetPublicKeysAsync();
        Task<SecurityKey> GetPrivateKeyAsync();
    }
}
