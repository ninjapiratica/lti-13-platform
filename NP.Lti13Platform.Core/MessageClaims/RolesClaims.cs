using Microsoft.Extensions.Logging;
using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Models;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.MessageClaims;

/// <summary>
/// Defines the contract for a roles message in LTI 1.3.
/// </summary>
public interface IRolesClaims
{
    /// <summary>
    /// Gets or sets the roles associated with the user.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/roles")]
    public IEnumerable<string> Roles { get; set; }
}

public static partial class ClaimsExtensions
{
    /// <summary>
    /// Populates the roles claims of the specified object using the roles from the provided membership information.
    /// </summary>
    /// <remarks>If a sub-role is present in the membership roles without its associated principal role, a
    /// warning is logged. Additionally, a warning is logged if the 'TestUser' system role is used without another valid
    /// role, in accordance with IMS Global LTI 1.3 best practices.</remarks>
    /// <typeparam name="T">The type of the object whose roles claims are to be filled. Must implement the IRolesClaims interface.</typeparam>
    /// <param name="obj">The object whose roles claims will be populated.</param>
    /// <param name="membership">The membership information containing the roles to assign to the object. Cannot be null.</param>
    /// <param name="logger">The logger used to record warnings about missing principal roles or improper role usage. Cannot be null.</param>
    /// <returns>The same object instance with its Roles property set to the roles from the membership.</returns>
    public static T WithRolesClaims<T>(
        this T obj,
        Membership membership,
        ILogger logger)
        where T : IRolesClaims
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

        return obj;
    }
}
