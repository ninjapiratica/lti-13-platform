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

    public class RolesPopulator(IDataService dataService) : Populator<IRolesMessage>
    {
        public override async Task PopulateAsync(IRolesMessage obj, Lti13MessageScope scope)
        {
            obj.Roles = await dataService.GetRolesAsync(scope.User.Id, scope.Context);

            if (obj.Roles.Contains(Lti13ContextRoles.Mentor))
            {
                obj.RoleScopeMentor = await dataService.GetMentoredUserIdsAsync(scope.User.Id, scope.Context);
            }
        }
    }
}
