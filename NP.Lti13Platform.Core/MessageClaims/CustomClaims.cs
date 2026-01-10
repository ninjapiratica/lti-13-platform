using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Extensions;
using NP.Lti13Platform.Core.Models;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.MessageClaims;

/// <summary>
/// Defines the contract for a message containing custom LTI 1.3 claims.
/// </summary>
public interface ICustomClaims
{
    /// <summary>
    /// Gets or sets the custom claims.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/custom")]
    public IDictionary<string, string>? Custom { get; set; }
}

public static partial class ClaimsExtensions
{
    /// <summary>
    /// Populates the custom claims of the specified object with values derived from the provided LTI 1.3 context, user, and
    /// resource information, according to the given custom permissions.
    /// </summary>
    /// <remarks>If any of the tool, deployment, or resource link parameters are null or do not contain custom
    /// claim definitions, no claims will be added. Only claims permitted by the customPermissions parameter are
    /// included. Existing custom claims on the object are merged with the new values.</remarks>
    /// <typeparam name="T">The type of the object to populate with custom claims. Must implement the ICustomClaims interface.</typeparam>
    /// <param name="obj">The object whose custom claims will be filled. Must implement ICustomClaims.</param>
    /// <param name="customPermissions">The set of permissions that determine which custom claim values are included.</param>
    /// <param name="platform">The platform information used to populate platform-related custom claims. May be null.</param>
    /// <param name="tool">The tool information used to provide tool-level custom claim values. May be null.</param>
    /// <param name="deployment">The deployment information used to provide deployment-level custom claim values. May be null.</param>
    /// <param name="context">The context information (such as course or group) used to populate context-related custom claims. May be null.</param>
    /// <param name="resourceLink">The resource link information used to populate resource link-related custom claims. May be null.</param>
    /// <param name="userMembership">The membership information for the user, used to provide user membership-related custom claims. May be null.</param>
    /// <param name="user">The user information used to populate user-related custom claims. May be null.</param>
    /// <param name="actualUserMembership">The membership information for the actual user (if different from the user), used to provide actual user membership-related custom claims. May be null.</param>
    /// <param name="actualUser">The actual user information, used to populate actual user-related custom claims. May be null.</param>
    /// <param name="lineItem">The line item information used to populate line item-related custom claims. May be null.</param>
    /// <param name="attempt">The attempt information used to populate attempt-related custom claims. May be null.</param>
    /// <param name="grade">The grade information used to populate grade-related custom claims. May be null.</param>
    /// <returns>The same object instance with its custom claims populated based on the provided context and permissions.</returns>
    public static T WithCustomClaims<T>(
        this T obj,
        CustomPermissions customPermissions,
        Platform? platform = null,
        Tool? tool = null,
        Deployment? deployment = null,
        Context? context = null,
        ResourceLink? resourceLink = null,
        Membership? userMembership = null,
        User? user = null,
        Membership? actualUserMembership = null,
        User? actualUser = null,
        LineItem? lineItem = null,
        Attempt? attempt = null,
        Grade? grade = null)
        where T : ICustomClaims
    {
        var customDictionary = tool?.Custom.Merge(deployment?.Custom).Merge(resourceLink?.Custom);

        if (customDictionary == null)
        {
            return obj;
        }

        var mentoredUserIds = userMembership?.Roles.Contains(Lti13ContextRoles.Mentor) == true
            ? userMembership.MentoredUserIds
            : null;
        var actualUserMentoredUserIds = actualUserMembership?.Roles.Contains(Lti13ContextRoles.Mentor) == true
            ? actualUserMembership.MentoredUserIds
            : null;

        foreach (var kvp in customDictionary.Where(kvp => kvp.Value.StartsWith('$')))
        {
            // TODO: LIS variables
            customDictionary[kvp.Key] = kvp.Value switch
            {
                Lti13UserVariables.Id when customPermissions.UserId => user?.Id.ToString(),
                Lti13UserVariables.Image when customPermissions.UserImage => user?.Picture?.OriginalString,
                Lti13UserVariables.Username when customPermissions.UserUsername => user?.Username,
                Lti13UserVariables.Org when customPermissions.UserOrg => user != null ? string.Join(',', user.Orgs) : string.Empty,
                Lti13UserVariables.ScopeMentor when customPermissions.UserScopeMentor => mentoredUserIds != null ? string.Join(',', mentoredUserIds) : string.Empty,
                Lti13UserVariables.GradeLevelsOneRoster when customPermissions.UserGradeLevelsOneRoster => user != null ? string.Join(',', user.OneRosterGrades) : string.Empty,

                Lti13ActualUserVariables.Id when customPermissions.ActualUserId => actualUser?.Id.ToString(),
                Lti13ActualUserVariables.Image when customPermissions.ActualUserImage => actualUser?.Picture?.OriginalString,
                Lti13ActualUserVariables.Username when customPermissions.ActualUserUsername => actualUser?.Username,
                Lti13ActualUserVariables.Org when customPermissions.ActualUserOrg => actualUser != null ? string.Join(',', actualUser.Orgs) : string.Empty,
                Lti13ActualUserVariables.ScopeMentor when customPermissions.ActualUserScopeMentor => actualUserMentoredUserIds != null ? string.Join(',', actualUserMentoredUserIds) : null,
                Lti13ActualUserVariables.GradeLevelsOneRoster when customPermissions.ActualUserGradeLevelsOneRoster => actualUser != null ? string.Join(',', actualUser.OneRosterGrades) : string.Empty,

                Lti13ContextVariables.Id when customPermissions.ContextId => context?.Id.ToString(),
                Lti13ContextVariables.Org when customPermissions.ContextOrg => context != null ? string.Join(',', context.Orgs) : string.Empty,
                Lti13ContextVariables.Type when customPermissions.ContextType => context != null ? string.Join(',', context.Types) : string.Empty,
                Lti13ContextVariables.Label when customPermissions.ContextLabel => context?.Label,
                Lti13ContextVariables.Title when customPermissions.ContextTitle => context?.Title,
                Lti13ContextVariables.SourcedId when customPermissions.ContextSourcedId => context?.SourcedId,
                Lti13ContextVariables.IdHistory when customPermissions.ContextIdHistory => context != null ? string.Join(',', context.ClonedIdHistory) : string.Empty,
                Lti13ContextVariables.GradeLevelsOneRoster when customPermissions.ContextGradeLevelsOneRoster => context != null ? string.Join(',', context.OneRosterGrades) : string.Empty,

                Lti13ResourceLinkVariables.Id when customPermissions.ResourceLinkId => resourceLink?.Id.ToString(),
                Lti13ResourceLinkVariables.Title when customPermissions.ResourceLinkTitle => resourceLink?.Title,
                Lti13ResourceLinkVariables.Description when customPermissions.ResourceLinkDescription => resourceLink?.Text,
                Lti13ResourceLinkVariables.AvailableStartDateTime when customPermissions.ResourceLinkAvailableStartDateTime => resourceLink?.AvailableStartDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.AvailableUserStartDateTime when customPermissions.ResourceLinkAvailableUserStartDateTime => attempt?.AvailableStartDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.AvailableEndDateTime when customPermissions.ResourceLinkAvailableEndDateTime => resourceLink?.AvailableEndDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.AvailableUserEndDateTime when customPermissions.ResourceLinkAvailableUserEndDateTime => attempt?.AvailableEndDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.SubmissionStartDateTime when customPermissions.ResourceLinkSubmissionStartDateTime => resourceLink?.SubmissionStartDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.SubmissionUserStartDateTime when customPermissions.ResourceLinkSubmissionUserStartDateTime => attempt?.SubmisstionStartDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.SubmissionEndDateTime when customPermissions.ResourceLinkSubmissionEndDateTime => resourceLink?.SubmissionEndDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.SubmissionUserEndDateTime when customPermissions.ResourceLinkSubmissionUserEndDateTime => attempt?.SubmissionEndDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.LineItemReleaseDateTime when customPermissions.ResourceLinkLineItemReleaseDateTime => lineItem?.GradesReleasedDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.LineItemUserReleaseDateTime when customPermissions.ResourceLinkLineItemUserReleaseDateTime => grade?.ReleaseDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.IdHistory when customPermissions.ResourceLinkIdHistory => resourceLink?.ClonedIdHistory != null ? string.Join(',', resourceLink.ClonedIdHistory) : string.Empty,

                Lti13ToolPlatformVariables.ProductFamilyCode when customPermissions.ToolPlatformProductFamilyCode => platform?.ProductFamilyCode,
                Lti13ToolPlatformVariables.Version when customPermissions.ToolPlatformProductVersion => platform?.Version,
                Lti13ToolPlatformVariables.InstanceGuid when customPermissions.ToolPlatformProductInstanceGuid => platform?.Guid,
                Lti13ToolPlatformVariables.InstanceName when customPermissions.ToolPlatformProductInstanceName => platform?.Name,
                Lti13ToolPlatformVariables.InstanceDescription when customPermissions.ToolPlatformProductInstanceDescription => platform?.Description,
                Lti13ToolPlatformVariables.InstanceUrl when customPermissions.ToolPlatformProductInstanceUrl => platform?.Url?.OriginalString,
                Lti13ToolPlatformVariables.InstanceContactEmail when customPermissions.ToolPlatformProductInstanceContactEmail => platform?.ContactEmail,
                _ => kvp.Value
            } ?? string.Empty;
        }

        obj.Custom = obj.Custom.Merge(customDictionary);

        return obj;
    }

}