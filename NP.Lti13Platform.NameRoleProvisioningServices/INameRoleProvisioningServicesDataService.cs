using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    public interface INameRoleProvisioningServicesDataService
    {
        Task<PartialList<Membership>> GetMembershipsAsync(string deploymnetId, string contextId, int pageIndex, int limit, string? role, string? resourceLinkId, DateTime? asOfDate = null);
        Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userIds, DateTime? asOfDate = null);
    }
}
