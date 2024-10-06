﻿using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Extensions;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Populators;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Populators
{
    public interface ICustomMessage
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/custom")]
        public IDictionary<string, string>? Custom { get; set; }
    }

    public class CustomPopulator(ICoreDataService dataService) : Populator<ICustomMessage>
    {
        private static readonly IEnumerable<string> LineItemAttemptGradeVariables = [
            Lti13ResourceLinkVariables.AvailableUserStartDateTime,
            Lti13ResourceLinkVariables.AvailableUserEndDateTime,
            Lti13ResourceLinkVariables.SubmissionUserStartDateTime,
            Lti13ResourceLinkVariables.SubmissionUserEndDateTime,
            Lti13ResourceLinkVariables.LineItemUserReleaseDateTime];

        public override async Task PopulateAsync(ICustomMessage obj, Lti13MessageScope scope)
        {
            var customDictionary = scope.Tool.Custom.Merge(scope.Deployment.Custom).Merge(scope.ResourceLink?.Custom);

            if (customDictionary == null)
            {
                return;
            }

            IEnumerable<string> mentoredUserIds = [];
            if (customDictionary.Values.Any(v => v == Lti13UserVariables.ScopeMentor) && scope.Context != null)
            {
                var membership = await dataService.GetMembershipAsync(scope.Context.Id, scope.User.Id);
                if (membership != null && membership.Roles.Contains(Lti13ContextRoles.Mentor))
                {
                    mentoredUserIds = await dataService.GetMentoredUserIdsAsync(scope.Context.Id, scope.User.Id);
                }
            }

            LineItem? lineItem = null;
            Attempt? attempt = null;
            Grade? grade = null;
            if (scope.Context != null && scope.ResourceLink != null && customDictionary.Values.Any(v => LineItemAttemptGradeVariables.Contains(v)))
            {
                var lineItems = await dataService.GetLineItemsAsync(scope.Deployment.Id, scope.Context.Id, pageIndex: 0, limit: 1, resourceLinkId: scope.ResourceLink.Id);
                if (lineItems.TotalItems == 1)
                {
                    lineItem = lineItems.Items.First();

                    grade = await dataService.GetGradeAsync(lineItem.Id, scope.User.Id);
                }

                attempt = await dataService.GetAttemptAsync(scope.ResourceLink.Id, scope.User.Id);
            }

            var dictionaryValues = customDictionary.ToList();
            foreach (var kvp in dictionaryValues)
            {
                var value = kvp.Value switch
                {
                    Lti13UserVariables.Id when scope.Tool.CustomPermissions.UserId => scope.User?.Id,
                    Lti13UserVariables.Image when scope.Tool.CustomPermissions.UserImage => scope.User?.ImageUrl,
                    Lti13UserVariables.Username when scope.Tool.CustomPermissions.UserUsername => scope.User?.Username,
                    Lti13UserVariables.Org when scope.Tool.CustomPermissions.UserOrg => scope.User != null ? string.Join(',', scope.User.Orgs) : string.Empty,
                    Lti13UserVariables.ScopeMentor when scope.Tool.CustomPermissions.UserScopeMentor => string.Join(',', mentoredUserIds),
                    Lti13UserVariables.GradeLevelsOneRoster when scope.Tool.CustomPermissions.UserGradeLevelsOneRoster => scope.User != null ? string.Join(',', scope.User.OneRosterGrades) : string.Empty,

                    Lti13ResourceLinkVariables.AvailableUserStartDateTime when scope.Tool.CustomPermissions.ResourceLinkAvailableUserStartDateTime => attempt?.AvailableStartDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.AvailableUserEndDateTime when scope.Tool.CustomPermissions.ResourceLinkAvailableUserEndDateTime => attempt?.AvailableEndDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.SubmissionUserStartDateTime when scope.Tool.CustomPermissions.ResourceLinkSubmissionUserStartDateTime => attempt?.SubmisstionStartDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.SubmissionUserEndDateTime when scope.Tool.CustomPermissions.ResourceLinkSubmissionUserEndDateTime => attempt?.SubmissionEndDateTime?.ToString("O"),
                    Lti13ResourceLinkVariables.LineItemUserReleaseDateTime when scope.Tool.CustomPermissions.ResourceLinkLineItemUserReleaseDateTime => grade?.ReleaseDateTime?.ToString("O"),
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
}