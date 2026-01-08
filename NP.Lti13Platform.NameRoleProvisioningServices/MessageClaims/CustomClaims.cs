using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Extensions;
using NP.Lti13Platform.Core.Models;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices.MessageClaims;

/// <summary>
/// Defines an interface for a message containing custom parameters.
/// </summary>
public interface INrpsCustomClaims
{
    /// <summary>
    /// Gets or sets the dictionary of custom parameters.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/custom")]
    public IDictionary<string, string>? Custom { get; set; }
}

/// <summary>
/// Populates custom parameters for LTI messages.
/// </summary>
public static partial class ClaimsExtensions
{
    /// <summary>
    /// Adds or updates custom claims on the specified object based on the provided permissions, tool, deployment, resource link, membership, user, attempt, and grade information.
    /// </summary>
    /// <remarks>If no custom claim definitions are present in the tool, deployment, or resource link, the object is returned unchanged.
    /// Only claims permitted by the specified permissions are included. Existing custom claims on the object are merged with the new claims.</remarks>
    /// <typeparam name="T">The type of the object to which custom claims are applied. Must implement <see cref="INrpsCustomClaims"/>.</typeparam>
    /// <param name="obj">The object to which custom claims will be added or updated.</param>
    /// <param name="customPermissions">The set of permissions that determine which custom claims are included.</param>
    /// <param name="tool">The tool instance containing custom claim definitions.</param>
    /// <param name="deployment">The deployment instance containing additional custom claim definitions.</param>
    /// <param name="resourceLink">The resource link instance that may provide further custom claim definitions. Can be null.</param>
    /// <param name="userMembership">The user's membership information, used to determine role-based claims. Can be null.</param>
    /// <param name="user">The user for whom claims are being generated.</param>
    /// <param name="attempt">The attempt information, used for claims related to availability and submission times. Can be null.</param>
    /// <param name="grade">The grade information, used for claims related to release dates. Can be null.</param>
    /// <returns>The original object with custom claims added or updated according to the provided context and permissions.</returns>
    public static T WithCustomClaims<T>(
        this T obj,
        CustomPermissions customPermissions,
        Tool tool,
        Deployment deployment,
        ResourceLink resourceLink,
        Membership userMembership,
        User user,
        Attempt? attempt,
        Grade? grade)
        where T : INrpsCustomClaims
    {
        var customDictionary = tool.Custom
            .Merge(deployment.Custom)
            .Merge(resourceLink.Custom);

        if (customDictionary == null)
        {
            return obj;
        }

        var mentoredUserIds = userMembership?.Roles.Contains(Lti13ContextRoles.Mentor) == true
            ? userMembership.MentoredUserIds
            : null;

        var dictionaryValues = customDictionary.ToList();
        foreach (var kvp in dictionaryValues)
        {
            var value = kvp.Value switch
            {
                Lti13UserVariables.Id when customPermissions.UserId => user.Id.ToString(),
                Lti13UserVariables.Image when customPermissions.UserImage => user.Picture?.OriginalString,
                Lti13UserVariables.Username when customPermissions.UserUsername => user.Username,
                Lti13UserVariables.Org when customPermissions.UserOrg => string.Join(',', user.Orgs),
                Lti13UserVariables.ScopeMentor when customPermissions.UserScopeMentor => mentoredUserIds != null ? string.Join(',', mentoredUserIds) : string.Empty,
                Lti13UserVariables.GradeLevelsOneRoster when customPermissions.UserGradeLevelsOneRoster => string.Join(',', user.OneRosterGrades),

                Lti13ResourceLinkVariables.AvailableUserStartDateTime when customPermissions.ResourceLinkAvailableUserStartDateTime => attempt?.AvailableStartDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.AvailableUserEndDateTime when customPermissions.ResourceLinkAvailableUserEndDateTime => attempt?.AvailableEndDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.SubmissionUserStartDateTime when customPermissions.ResourceLinkSubmissionUserStartDateTime => attempt?.SubmisstionStartDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.SubmissionUserEndDateTime when customPermissions.ResourceLinkSubmissionUserEndDateTime => attempt?.SubmissionEndDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.LineItemUserReleaseDateTime when customPermissions.ResourceLinkLineItemUserReleaseDateTime => grade?.ReleaseDateTime?.ToString("O"),
                _ => null
            };

            if (value == null)
            {
                customDictionary.Remove(kvp.Key);
            }
            else
            {
                customDictionary[kvp.Key] = value;
            }
        }

        obj.Custom = obj.Custom.Merge(customDictionary);

        return obj;
    }
}
