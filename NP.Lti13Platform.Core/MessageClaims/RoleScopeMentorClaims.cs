using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Models;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.MessageClaims;

/// <summary>
/// Represents a contract for accessing the collection of users mentored by the current user within an LTI role scope context.
/// </summary>
/// <remarks>Implementations of this interface provide access to mentor relationships as defined by the LTI
/// specification. The collection may be empty if the user does not mentor any other users. This interface is typically
/// used in educational platforms to determine which users a mentor can act on behalf of.</remarks>
public interface IRoleScopeMentorClaims
{
    /// <summary>
    /// Gets or sets the users being mentored by this user.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/role_scope_mentor")]
    public IEnumerable<UserId>? RoleScopeMentor { get; set; }
}

public static partial class ClaimsExtensions
{
    /// <summary>
    /// Populates the RoleScopeMentor property with mentored user IDs if the membership includes the Mentor role.
    /// </summary>
    /// <remarks>If the membership does not include the Mentor role, the RoleScopeMentor property is not modified.</remarks>
    /// <typeparam name="T">The type of the object to populate. Must implement IRoleScopeMentorClaims.</typeparam>
    /// <param name="obj">The object whose RoleScopeMentor property will be set if the Mentor role is present.</param>
    /// <param name="membership">The membership information used to determine roles and mentored user IDs. Cannot be null.</param>
    /// <returns>The same object instance with the RoleScopeMentor property set if applicable.</returns>
    public static T WithRoleScopeMentorClaims<T>(
        this T obj,
        Membership membership)
        where T : IRoleScopeMentorClaims
    {
        if (membership.Roles.Contains(Lti13ContextRoles.Mentor))
        {
            obj.RoleScopeMentor = membership.MentoredUserIds;
        }

        return obj;
    }
}

