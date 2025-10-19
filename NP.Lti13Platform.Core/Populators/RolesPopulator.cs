using Microsoft.Extensions.Logging;
using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.Populators;

/// <summary>
/// Defines the contract for a roles message in LTI 1.3.
/// </summary>
public interface IRolesMessage
{
    /// <summary>
    /// Gets or sets the roles associated with the user.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/roles")]
    public IEnumerable<string> Roles { get; set; }

    /// <summary>
    /// Gets or sets the users being mentored by this user.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/role_scope_mentor")]
    public IEnumerable<UserId>? RoleScopeMentor { get; set; }
}

/// <summary>
/// Populates a roles message with information from the message scope.
/// </summary>
public class RolesPopulator(ILti13CoreDataService dataService, ILogger<RolesPopulator> logger) : Populator<IRolesMessage>
{
    /// <summary>
    /// Populates a roles message with information from the message scope.
    /// </summary>
    /// <param name="obj">The roles message to populate.</param>
    /// <param name="scope">The message scope containing the context and user information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task PopulateAsync(IRolesMessage obj, MessageScope scope, CancellationToken cancellationToken = default)
    {
        if (scope.Context != null)
        {
            var membership = await dataService.GetMembershipAsync(scope.Context.Id, scope.UserScope.User.Id, cancellationToken);
            if (membership != null)
            {
                // Whenever a platform specifies a sub-role, by best practice it should also include the associated principal role.
                // https://www.imsglobal.org/spec/lti/v1p3/#context-sub-roles
                if (membership.Roles.Any())
                {
                    foreach (var subRole in membership.Roles.Where(r => r.StartsWith("http://purl.imsglobal.org/vocab/lis/v2/membership/") && r.Contains('#')))
                    {
                        var principalRole = subRole.Split('#').First();
                        var index = principalRole.LastIndexOf('/');
                        principalRole = $"{principalRole[..index]}#{principalRole[(index + 1)..]}";

                        if (!membership.Roles.Contains(principalRole))
                        {
                            logger.LogWarning("Sub-role {SubRole} is missing its principal role {PrincipalRole}. https://www.imsglobal.org/spec/lti/v1p3/#context-sub-roles", subRole, principalRole);
                        }
                    }

                    if (membership.Roles.SequenceEqual([Lti13SystemRoles.TestUser]))
                    {
                        logger.LogWarning("{TestUser} system role should be used only in conjunction with a 'real' role. https://www.imsglobal.org/spec/lti/v1p3/#lti-vocabulary-for-system-roles.", Lti13SystemRoles.TestUser);
                    }
                }

                obj.Roles = membership.Roles;

                if (obj.Roles.Contains(Lti13ContextRoles.Mentor))
                {
                    obj.RoleScopeMentor = membership.MentoredUserIds;
                }
            }
        }
    }
}
