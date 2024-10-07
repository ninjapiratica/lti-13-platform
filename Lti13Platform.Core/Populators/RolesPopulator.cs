using System.Text.Json.Serialization;

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
        public override async Task PopulateAsync(IRolesMessage obj, Lti13MessageScope scope)
        {
            if (scope.Context != null && scope.User != null)
            {
                var membership = await dataService.GetMembershipAsync(scope.Context.Id, scope.User.Id);
                if (membership != null)
                {
                    obj.Roles = membership.Roles;

                    if (obj.Roles.Contains(Lti13ContextRoles.Mentor))
                    {
                        obj.RoleScopeMentor = await dataService.GetMentoredUserIdsAsync(scope.Context.Id, scope.User.Id);
                    }
                }
            }
        }
    }
}
