using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Services
{
    public interface INameRoleProvisioningDataService
    {
        Task<PartialList<Membership>> GetMembershipsAsync(string deploymnetId, string contextId, int pageIndex, int limit, string? role, string? resourceLinkId, DateTime? asOfDate = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userIds, DateTime? asOfDate = null, CancellationToken cancellationToken = default);
    }
}
