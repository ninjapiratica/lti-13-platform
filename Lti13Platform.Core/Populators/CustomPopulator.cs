using NP.Lti13Platform.Core.Extensions;
using NP.Lti13Platform.Core.Models;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.Populators
{
    public interface ICustomMessage
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/custom")]
        public IDictionary<string, string>? Custom { get; set; }
    }

    public class CustomPopulator(IPlatformService platformService, ICoreDataService dataService) : Populator<ICustomMessage>
    {
        private static readonly IEnumerable<string> LineItemAttemptGradeVariables = [
            Lti13ResourceLinkVariables.AvailableUserStartDateTime,
            Lti13ResourceLinkVariables.AvailableUserEndDateTime,
            Lti13ResourceLinkVariables.SubmissionUserStartDateTime,
            Lti13ResourceLinkVariables.SubmissionUserEndDateTime,
            Lti13ResourceLinkVariables.LineItemReleaseDateTime,
            Lti13ResourceLinkVariables.LineItemUserReleaseDateTime];

        public override async Task PopulateAsync(ICustomMessage obj, Lti13MessageScope scope)
        {
            var customDictionary = scope.Tool.Custom.Merge(scope.Deployment.Custom).Merge(scope.ResourceLink?.Custom);

            if (customDictionary == null)
            {
                return;
            }

            Platform? platform = null;
            if (customDictionary.Values.Any(v => v.StartsWith(Lti13ToolPlatformVariables.Version.Split('.')[0])) == true)
            {
                platform = await platformService.GetPlatformAsync(scope.Tool.ClientId);
            }

            IEnumerable<string> mentoredUserIds = [];
            if (customDictionary.Values.Any(v => v == Lti13UserVariables.ScopeMentor) && scope.Context != null && scope.User != null)
            {
                var membership = await dataService.GetMembershipAsync(scope.Context.Id, scope.User.Id);
                if (membership != null && membership.Roles.Contains(Lti13ContextRoles.Mentor))
                {
                    mentoredUserIds = await dataService.GetMentoredUserIdsAsync(scope.Context.Id, scope.User.Id);
                }
            }

            IEnumerable<string> actualUserMentoredUserIds = [];
            if (customDictionary.Values.Any(v => v == Lti13ActualUserVariables.ScopeMentor) && scope.Context != null && scope.ActualUser != null)
            {
                var membership = await dataService.GetMembershipAsync(scope.Context.Id, scope.ActualUser.Id);
                if (membership != null && membership.Roles.Contains(Lti13ContextRoles.Mentor))
                {
                    actualUserMentoredUserIds = await dataService.GetMentoredUserIdsAsync(scope.Context.Id, scope.ActualUser.Id);
                }
            }

            LineItem? lineItem = null;
            Attempt? attempt = null;
            Grade? grade = null;
            if (customDictionary.Values.Any(v => LineItemAttemptGradeVariables.Contains(v)) && scope.Context != null && scope.User != null && scope.ResourceLink != null)
            {
                var lineItems = await dataService.GetLineItemsAsync(scope.Deployment.Id, scope.Context.Id, 0, 1, null, scope.ResourceLink.Id, null);
                if (lineItems.TotalItems == 1)
                {
                    lineItem = lineItems.Items.Single();

                    grade = await dataService.GetGradeAsync(lineItem.Id, scope.User.Id);
                }

                attempt = await dataService.GetAttemptAsync(scope.ResourceLink.Id, scope.User.Id);
            }

            foreach (var kvp in customDictionary.Where(kvp => kvp.Value.StartsWith('$')))
            {
                // TODO: LIS variables
                customDictionary[kvp.Key] = kvp.Value switch
                {
                    Lti13UserVariables.Id when scope.Tool.CustomPermissions.UserId => scope.User?.Id,
                    Lti13UserVariables.Image when scope.Tool.CustomPermissions.UserImage => scope.User?.ImageUrl,
                    Lti13UserVariables.Username when scope.Tool.CustomPermissions.UserUsername => scope.User?.Username,
                    Lti13UserVariables.Org when scope.Tool.CustomPermissions.UserOrg => scope.User != null ? string.Join(',', scope.User.Orgs) : string.Empty,
                    Lti13UserVariables.ScopeMentor when scope.Tool.CustomPermissions.UserScopeMentor => string.Join(',', mentoredUserIds),
                    Lti13UserVariables.GradeLevelsOneRoster when scope.Tool.CustomPermissions.UserGradeLevelsOneRoster => scope.User != null ? string.Join(',', scope.User.OneRosterGrades) : string.Empty,

                    Lti13ActualUserVariables.Id when scope.Tool.CustomPermissions.ActualUserId => scope.ActualUser?.Id,
                    Lti13ActualUserVariables.Image when scope.Tool.CustomPermissions.ActualUserImage => scope.ActualUser?.ImageUrl,
                    Lti13ActualUserVariables.Username when scope.Tool.CustomPermissions.ActualUserUsername => scope.ActualUser?.Username,
                    Lti13ActualUserVariables.Org when scope.Tool.CustomPermissions.ActualUserOrg => scope.ActualUser != null ? string.Join(',', scope.ActualUser.Orgs) : string.Empty,
                    Lti13ActualUserVariables.ScopeMentor when scope.Tool.CustomPermissions.ActualUserScopeMentor => string.Join(',', actualUserMentoredUserIds),
                    Lti13ActualUserVariables.GradeLevelsOneRoster when scope.Tool.CustomPermissions.ActualUserGradeLevelsOneRoster => scope.ActualUser != null ? string.Join(',', scope.ActualUser.OneRosterGrades) : string.Empty,

                    Lti13ContextVariables.Id when scope.Tool.CustomPermissions.ContextId => scope.Context?.Id,
                    Lti13ContextVariables.Org when scope.Tool.CustomPermissions.ContextOrg => scope.Context != null ? string.Join(',', scope.Context.Orgs) : string.Empty,
                    Lti13ContextVariables.Type when scope.Tool.CustomPermissions.ContextType => scope.Context != null ? string.Join(',', scope.Context.Types) : string.Empty,
                    Lti13ContextVariables.Label when scope.Tool.CustomPermissions.ContextLabel => scope.Context?.Label,
                    Lti13ContextVariables.Title when scope.Tool.CustomPermissions.ContextTitle => scope.Context?.Title,
                    Lti13ContextVariables.SourcedId when scope.Tool.CustomPermissions.ContextSourcedId => scope.Context?.SourcedId,
                    Lti13ContextVariables.IdHistory when scope.Tool.CustomPermissions.ContextIdHistory => scope.Context != null ? string.Join(',', scope.Context.ClonedIdHistory) : string.Empty,
                    Lti13ContextVariables.GradeLevelsOneRoster when scope.Tool.CustomPermissions.ContextGradeLevelsOneRoster => scope.Context != null ? string.Join(',', scope.Context.OneRosterGrades) : string.Empty,

                    Lti13ResourceLinkVariables.Id when scope.Tool.CustomPermissions.ResourceLinkId => scope.ResourceLink?.Id,
                    Lti13ResourceLinkVariables.Title when scope.Tool.CustomPermissions.ResourceLinkTitle => scope.ResourceLink?.Title,
                    Lti13ResourceLinkVariables.Description when scope.Tool.CustomPermissions.ResourceLinkDescription => scope.ResourceLink?.Text,
                    Lti13ResourceLinkVariables.AvailableStartDateTime when scope.Tool.CustomPermissions.ResourceLinkAvailableStartDateTime => scope.ResourceLink?.AvailableStartDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.AvailableUserStartDateTime when scope.Tool.CustomPermissions.ResourceLinkAvailableUserStartDateTime => attempt?.AvailableStartDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.AvailableEndDateTime when scope.Tool.CustomPermissions.ResourceLinkAvailableEndDateTime => scope.ResourceLink?.AvailableEndDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.AvailableUserEndDateTime when scope.Tool.CustomPermissions.ResourceLinkAvailableUserEndDateTime => attempt?.AvailableEndDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.SubmissionStartDateTime when scope.Tool.CustomPermissions.ResourceLinkSubmissionStartDateTime => scope.ResourceLink?.SubmissionStartDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.SubmissionUserStartDateTime when scope.Tool.CustomPermissions.ResourceLinkSubmissionUserStartDateTime => attempt?.SubmisstionStartDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.SubmissionEndDateTime when scope.Tool.CustomPermissions.ResourceLinkSubmissionEndDateTime => scope.ResourceLink?.SubmissionEndDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.SubmissionUserEndDateTime when scope.Tool.CustomPermissions.ResourceLinkSubmissionUserEndDateTime => attempt?.SubmissionEndDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.LineItemReleaseDateTime when scope.Tool.CustomPermissions.ResourceLinkLineItemReleaseDateTime => lineItem?.GradesReleasedDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.LineItemUserReleaseDateTime when scope.Tool.CustomPermissions.ResourceLinkLineItemUserReleaseDateTime => grade?.ReleaseDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.IdHistory when scope.Tool.CustomPermissions.ResourceLinkIdHistory => scope.ResourceLink?.ClonedIdHistory != null ? string.Join(',', scope.ResourceLink.ClonedIdHistory) : string.Empty,

                    Lti13ToolPlatformVariables.ProductFamilyCode when scope.Tool.CustomPermissions.ToolPlatformProductFamilyCode => platform?.ProductFamilyCode,
                    Lti13ToolPlatformVariables.Version when scope.Tool.CustomPermissions.ToolPlatformProductVersion => platform?.Version,
                    Lti13ToolPlatformVariables.InstanceGuid when scope.Tool.CustomPermissions.ToolPlatformProductInstanceGuid => platform?.Guid,
                    Lti13ToolPlatformVariables.InstanceName when scope.Tool.CustomPermissions.ToolPlatformProductInstanceName => platform?.Name,
                    Lti13ToolPlatformVariables.InstanceDescription when scope.Tool.CustomPermissions.ToolPlatformProductInstanceDescription => platform?.Description,
                    Lti13ToolPlatformVariables.InstanceUrl when scope.Tool.CustomPermissions.ToolPlatformProductInstanceUrl => platform?.Url,
                    Lti13ToolPlatformVariables.InstanceContactEmail when scope.Tool.CustomPermissions.ToolPlatformProductInstanceContactEmail => platform?.ContactEmail,
                    _ => kvp.Value
                } ?? string.Empty;
            }

            obj.Custom = obj.Custom.Merge(customDictionary);
        }
    }
}
