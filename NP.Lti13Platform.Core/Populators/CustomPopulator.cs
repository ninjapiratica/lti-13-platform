using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Extensions;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.Populators;

/// <summary>
/// Defines the contract for a message containing custom LTI claims.
/// </summary>
public interface ICustomMessage
{
    /// <summary>
    /// Gets or sets the custom claims.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/custom")]
    public IDictionary<string, string>? Custom { get; set; }
}

/// <summary>
/// Populates the <see cref="ICustomMessage"/> with custom claims.
/// </summary>
/// <param name="platformService">The platform service.</param>
/// <param name="dataService">The core data service.</param>
public class CustomPopulator(ILti13PlatformService platformService, ILti13CoreDataService dataService) : Populator<ICustomMessage>
{
    private static readonly IEnumerable<string> LineItemAttemptGradeVariables = [
        Lti13ResourceLinkVariables.AvailableUserStartDateTime,
        Lti13ResourceLinkVariables.AvailableUserEndDateTime,
        Lti13ResourceLinkVariables.SubmissionUserStartDateTime,
        Lti13ResourceLinkVariables.SubmissionUserEndDateTime,
        Lti13ResourceLinkVariables.LineItemReleaseDateTime,
        Lti13ResourceLinkVariables.LineItemUserReleaseDateTime];

    /// <inheritdoc />
    public override async Task PopulateAsync(ICustomMessage obj, MessageScope scope, CancellationToken cancellationToken = default)
    {
        var customDictionary = scope.Tool.Custom.Merge(scope.Deployment.Custom).Merge(scope.ResourceLink?.Custom);

        if (customDictionary == null)
        {
            return;
        }

        Platform? platform = null;
        if (customDictionary.Values.Any(v => v.StartsWith(Lti13ToolPlatformVariables.Version.Split('.')[0])) == true)
        {
            platform = await platformService.GetPlatformAsync(scope.Tool.ClientId, cancellationToken);
        }

        IEnumerable<string> mentoredUserIds = [];
        if (customDictionary.Values.Any(v => v == Lti13UserVariables.ScopeMentor) && scope.Context != null)
        {
            var membership = await dataService.GetMembershipAsync(scope.Context.Id, scope.UserScope.User.Id, cancellationToken);
            if (membership != null && membership.Roles.Contains(Lti13ContextRoles.Mentor))
            {
                mentoredUserIds = membership.MentoredUserIds;
            }
        }

        IEnumerable<string> actualUserMentoredUserIds = [];
        if (customDictionary.Values.Any(v => v == Lti13ActualUserVariables.ScopeMentor) && scope.Context != null && scope.UserScope.ActualUser != null)
        {
            var membership = await dataService.GetMembershipAsync(scope.Context.Id, scope.UserScope.ActualUser.Id, cancellationToken);
            if (membership != null && membership.Roles.Contains(Lti13ContextRoles.Mentor))
            {
                actualUserMentoredUserIds = membership.MentoredUserIds;
            }
        }

        LineItem? lineItem = null;
        Attempt? attempt = null;
        Grade? grade = null;
        if (customDictionary.Values.Any(v => LineItemAttemptGradeVariables.Contains(v)) && scope.Context != null && scope.ResourceLink != null)
        {
            var lineItems = await dataService.GetLineItemsAsync(scope.Deployment.Id, scope.Context.Id, 0, 1, null, scope.ResourceLink.Id, null, cancellationToken);
            if (lineItems.TotalItems == 1)
            {
                lineItem = lineItems.Items.Single();

                grade = await dataService.GetGradeAsync(lineItem.Id, scope.UserScope.User.Id, cancellationToken);
            }

            attempt = await dataService.GetAttemptAsync(scope.ResourceLink.Id, scope.UserScope.User.Id, cancellationToken);
        }

        var customPermissions = await dataService.GetCustomPermissions(scope.Deployment.Id, scope.Context?.Id, scope.UserScope.User.Id, scope.UserScope.ActualUser?.Id, cancellationToken);

        foreach (var kvp in customDictionary.Where(kvp => kvp.Value.StartsWith('$')))
        {
            // TODO: LIS variables
            customDictionary[kvp.Key] = kvp.Value switch
            {
                Lti13UserVariables.Id when customPermissions.UserId && !scope.UserScope.IsAnonymous => scope.UserScope.User.Id,
                Lti13UserVariables.Image when customPermissions.UserImage && !scope.UserScope.IsAnonymous => scope.UserScope.User.Picture?.OriginalString,
                Lti13UserVariables.Username when customPermissions.UserUsername && !scope.UserScope.IsAnonymous => scope.UserScope.User.Username,
                Lti13UserVariables.Org when customPermissions.UserOrg && !scope.UserScope.IsAnonymous => string.Join(',', scope.UserScope.User.Orgs),
                Lti13UserVariables.ScopeMentor when customPermissions.UserScopeMentor && !scope.UserScope.IsAnonymous => string.Join(',', mentoredUserIds),
                Lti13UserVariables.GradeLevelsOneRoster when customPermissions.UserGradeLevelsOneRoster && !scope.UserScope.IsAnonymous => string.Join(',', scope.UserScope.User.OneRosterGrades),

                Lti13ActualUserVariables.Id when customPermissions.ActualUserId && !scope.UserScope.IsAnonymous => scope.UserScope.ActualUser?.Id,
                Lti13ActualUserVariables.Image when customPermissions.ActualUserImage && !scope.UserScope.IsAnonymous => scope.UserScope.ActualUser?.Picture?.OriginalString,
                Lti13ActualUserVariables.Username when customPermissions.ActualUserUsername && !scope.UserScope.IsAnonymous => scope.UserScope.ActualUser?.Username,
                Lti13ActualUserVariables.Org when customPermissions.ActualUserOrg && !scope.UserScope.IsAnonymous => scope.UserScope.ActualUser != null ? string.Join(',', scope.UserScope.ActualUser.Orgs) : string.Empty,
                Lti13ActualUserVariables.ScopeMentor when customPermissions.ActualUserScopeMentor && !scope.UserScope.IsAnonymous => string.Join(',', actualUserMentoredUserIds),
                Lti13ActualUserVariables.GradeLevelsOneRoster when customPermissions.ActualUserGradeLevelsOneRoster && !scope.UserScope.IsAnonymous => scope.UserScope.ActualUser != null ? string.Join(',', scope.UserScope.ActualUser.OneRosterGrades) : string.Empty,

                Lti13ContextVariables.Id when customPermissions.ContextId => scope.Context?.Id.ToString(),
                Lti13ContextVariables.Org when customPermissions.ContextOrg => scope.Context != null ? string.Join(',', scope.Context.Orgs) : string.Empty,
                Lti13ContextVariables.Type when customPermissions.ContextType => scope.Context != null ? string.Join(',', scope.Context.Types) : string.Empty,
                Lti13ContextVariables.Label when customPermissions.ContextLabel => scope.Context?.Label,
                Lti13ContextVariables.Title when customPermissions.ContextTitle => scope.Context?.Title,
                Lti13ContextVariables.SourcedId when customPermissions.ContextSourcedId => scope.Context?.SourcedId,
                Lti13ContextVariables.IdHistory when customPermissions.ContextIdHistory => scope.Context != null ? string.Join(',', scope.Context.ClonedIdHistory) : string.Empty,
                Lti13ContextVariables.GradeLevelsOneRoster when customPermissions.ContextGradeLevelsOneRoster => scope.Context != null ? string.Join(',', scope.Context.OneRosterGrades) : string.Empty,

                Lti13ResourceLinkVariables.Id when customPermissions.ResourceLinkId => scope.ResourceLink?.Id,
                Lti13ResourceLinkVariables.Title when customPermissions.ResourceLinkTitle => scope.ResourceLink?.Title,
                Lti13ResourceLinkVariables.Description when customPermissions.ResourceLinkDescription => scope.ResourceLink?.Text,
                Lti13ResourceLinkVariables.AvailableStartDateTime when customPermissions.ResourceLinkAvailableStartDateTime => scope.ResourceLink?.AvailableStartDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.AvailableUserStartDateTime when customPermissions.ResourceLinkAvailableUserStartDateTime => attempt?.AvailableStartDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.AvailableEndDateTime when customPermissions.ResourceLinkAvailableEndDateTime => scope.ResourceLink?.AvailableEndDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.AvailableUserEndDateTime when customPermissions.ResourceLinkAvailableUserEndDateTime => attempt?.AvailableEndDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.SubmissionStartDateTime when customPermissions.ResourceLinkSubmissionStartDateTime => scope.ResourceLink?.SubmissionStartDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.SubmissionUserStartDateTime when customPermissions.ResourceLinkSubmissionUserStartDateTime => attempt?.SubmisstionStartDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.SubmissionEndDateTime when customPermissions.ResourceLinkSubmissionEndDateTime => scope.ResourceLink?.SubmissionEndDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.SubmissionUserEndDateTime when customPermissions.ResourceLinkSubmissionUserEndDateTime => attempt?.SubmissionEndDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.LineItemReleaseDateTime when customPermissions.ResourceLinkLineItemReleaseDateTime => lineItem?.GradesReleasedDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.LineItemUserReleaseDateTime when customPermissions.ResourceLinkLineItemUserReleaseDateTime => grade?.ReleaseDateTime?.ToString("O"),
                Lti13ResourceLinkVariables.IdHistory when customPermissions.ResourceLinkIdHistory => scope.ResourceLink?.ClonedIdHistory != null ? string.Join(',', scope.ResourceLink.ClonedIdHistory) : string.Empty,

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
    }
}
