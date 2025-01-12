using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Extensions;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Populators;
using NP.Lti13Platform.Core.Services;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Populators;

public interface ICustomMessage
{
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/custom")]
    public IDictionary<string, string>? Custom { get; set; }
}

public class CustomPopulator(ILti13CoreDataService dataService) : Populator<ICustomMessage>
{
    private static readonly IEnumerable<string> LineItemAttemptGradeVariables = [
        Lti13ResourceLinkVariables.AvailableUserStartDateTime,
        Lti13ResourceLinkVariables.AvailableUserEndDateTime,
        Lti13ResourceLinkVariables.SubmissionUserStartDateTime,
        Lti13ResourceLinkVariables.SubmissionUserEndDateTime,
        Lti13ResourceLinkVariables.LineItemUserReleaseDateTime];

    public override async Task PopulateAsync(ICustomMessage obj, MessageScope scope, CancellationToken cancellationToken = default)
    {
        var customDictionary = scope.Tool.Custom.Merge(scope.Deployment.Custom).Merge(scope.ResourceLink?.Custom);

        if (customDictionary == null)
        {
            return;
        }

        IEnumerable<string> mentoredUserIds = [];
        if (customDictionary.Values.Any(v => v == Lti13UserVariables.ScopeMentor) && scope.Context != null )
        {
            var membership = await dataService.GetMembershipAsync(scope.Context.Id, scope.UserScope.User.Id, cancellationToken);
            if (membership != null && membership.Roles.Contains(Lti13ContextRoles.Mentor))
            {
                mentoredUserIds = membership.MentoredUserIds;
            }
        }

        LineItem? lineItem = null;
        Attempt? attempt = null;
        Grade? grade = null;
        if (customDictionary.Values.Any(v => LineItemAttemptGradeVariables.Contains(v)) && scope.Context != null && scope.ResourceLink != null)
        {
            var lineItems = await dataService.GetLineItemsAsync(scope.Deployment.Id, scope.Context.Id, pageIndex: 0, limit: 1, resourceLinkId: scope.ResourceLink.Id, cancellationToken: cancellationToken);
            if (lineItems.TotalItems == 1)
            {
                lineItem = lineItems.Items.First();

                grade = await dataService.GetGradeAsync(lineItem.Id, scope.UserScope.User.Id, cancellationToken);
            }

            attempt = await dataService.GetAttemptAsync(scope.ResourceLink.Id, scope.UserScope.User.Id, cancellationToken);
        }

        var customPermissions = await dataService.GetCustomPermissions(scope.Deployment.Id, cancellationToken);

        var dictionaryValues = customDictionary.ToList();
        foreach (var kvp in dictionaryValues)
        {
            var value = kvp.Value switch
            {
                Lti13UserVariables.Id when customPermissions.UserId => scope.UserScope.User.Id,
                Lti13UserVariables.Image when customPermissions.UserImage => scope.UserScope.User.Picture,
                Lti13UserVariables.Username when customPermissions.UserUsername => scope.UserScope.User.Username,
                Lti13UserVariables.Org when customPermissions.UserOrg => string.Join(',', scope.UserScope.User.Orgs),
                Lti13UserVariables.ScopeMentor when customPermissions.UserScopeMentor => string.Join(',', mentoredUserIds),
                Lti13UserVariables.GradeLevelsOneRoster when customPermissions.UserGradeLevelsOneRoster => string.Join(',', scope.UserScope.User.OneRosterGrades),

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
    }
}
