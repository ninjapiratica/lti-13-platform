using System.Text.Json.Serialization;
using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Services;

namespace NP.Lti13Platform.Core.Populators
{
    public interface IRolesMessage
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/roles")]
        public IEnumerable<string> Roles { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/role_scope_mentor")]
        public IEnumerable<string>? RoleScopeMentor { get; set; }
    }

    public class RolesPopulator(ICoreDataService dataService) : Populator<IRolesMessage>
    {
        public override async Task PopulateAsync(IRolesMessage obj, MessageScope scope, CancellationToken cancellationToken = default)
        {
            if (scope.Context != null)
            {
                var membership = await dataService.GetMembershipAsync(scope.Context.Id, scope.UserScope.User.Id, cancellationToken);
                if (membership != null)
                {
                    obj.Roles = membership.Roles;

                    if (obj.Roles.Contains(Lti13ContextRoles.Mentor))
                    {
                        obj.RoleScopeMentor = await dataService.GetMentoredUserIdsAsync(scope.Context.Id, scope.UserScope.User.Id, cancellationToken);
                    }
                }
            }
        }
    }
}
