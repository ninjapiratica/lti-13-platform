﻿using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services
{
    public interface ILti13CoreDataService
    {
        Task<Tool?> GetToolAsync(string clientId, CancellationToken cancellationToken = default);
        Task<Deployment?> GetDeploymentAsync(string deploymentId, CancellationToken cancellationToken = default);
        Task<Context?> GetContextAsync(string contextId, CancellationToken cancellationToken = default);
        Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<Membership?> GetMembershipAsync(string contextId, string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetMentoredUserIdsAsync(string contextId, string userId, CancellationToken cancellationToken = default);
        Task<ResourceLink?> GetResourceLinkAsync(string resourceLinkId, CancellationToken cancellationToken = default);

        Task<PartialList<LineItem>> GetLineItemsAsync(string deploymentId, string contextId, int pageIndex, int limit, string? resourceId = null, string? resourceLinkId = null, string? tag = null, CancellationToken cancellationToken = default);

        Task<Attempt?> GetAttemptAsync(string resourceLinkId, string userId, CancellationToken cancellationToken = default);

        Task<Grade?> GetGradeAsync(string lineItemId, string userId, CancellationToken cancellationToken = default);

        Task<ServiceToken?> GetServiceTokenRequestAsync(string toolId, string id, CancellationToken cancellationToken = default);
        Task SaveServiceTokenRequestAsync(ServiceToken serviceToken, CancellationToken cancellationToken = default);

        Task<IEnumerable<SecurityKey>> GetPublicKeysAsync(CancellationToken cancellationToken = default);
        Task<SecurityKey> GetPrivateKeyAsync(CancellationToken cancellationToken = default);
    }
}