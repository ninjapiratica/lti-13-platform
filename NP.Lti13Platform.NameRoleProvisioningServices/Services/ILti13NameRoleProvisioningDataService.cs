using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Services
{
    public interface ILti13NameRoleProvisioningDataService
    {
        Task<PartialList<(Membership Membership, User User)>> GetMembershipsAsync(string deploymentId, string contextId, int pageIndex, int limit, string? role, string? resourceLinkId, DateTime? asOfDate = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<(string UserId, UserPermissions UserPermissions)>> GetUserPermissionsAsync(string deploymentId, IEnumerable<string> userIds, CancellationToken cancellationToken = default);
    }
}
