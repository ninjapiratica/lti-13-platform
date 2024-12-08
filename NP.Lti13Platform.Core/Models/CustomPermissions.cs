namespace NP.Lti13Platform.Core.Models
{
    public class CustomPermissions
    {
        public bool UserId { get; set; }
        public bool UserImage { get; set; }
        public bool UserUsername { get; set; }
        public bool UserOrg { get; set; }
        public bool UserScopeMentor { get; set; }
        public bool UserGradeLevelsOneRoster { get; set; }

        public bool ActualUserId { get; set; }
        public bool ActualUserImage { get; set; }
        public bool ActualUserUsername { get; set; }
        public bool ActualUserOrg { get; set; }
        public bool ActualUserScopeMentor { get; set; }
        public bool ActualUserGradeLevelsOneRoster { get; set; }

        public bool ContextId { get; set; }
        public bool ContextOrg { get; set; }
        public bool ContextType { get; set; }
        public bool ContextLabel { get; set; }
        public bool ContextTitle { get; set; }
        public bool ContextSourcedId { get; set; }
        public bool ContextIdHistory { get; set; }
        public bool ContextGradeLevelsOneRoster { get; set; }

        public bool ResourceLinkId { get; set; }
        public bool ResourceLinkTitle { get; set; }
        public bool ResourceLinkDescription { get; set; }
        public bool ResourceLinkAvailableStartDateTime { get; set; }
        public bool ResourceLinkAvailableUserStartDateTime { get; set; }
        public bool ResourceLinkAvailableEndDateTime { get; set; }
        public bool ResourceLinkAvailableUserEndDateTime { get; set; }
        public bool ResourceLinkSubmissionStartDateTime { get; set; }
        public bool ResourceLinkSubmissionUserStartDateTime { get; set; }
        public bool ResourceLinkSubmissionEndDateTime { get; set; }
        public bool ResourceLinkSubmissionUserEndDateTime { get; set; }
        public bool ResourceLinkLineItemReleaseDateTime { get; set; }
        public bool ResourceLinkLineItemUserReleaseDateTime { get; set; }
        public bool ResourceLinkIdHistory { get; set; }

        public bool ToolPlatformProductFamilyCode { get; set; }
        public bool ToolPlatformProductVersion { get; set; }
        public bool ToolPlatformProductInstanceGuid { get; set; }
        public bool ToolPlatformProductInstanceName { get; set; }
        public bool ToolPlatformProductInstanceDescription { get; set; }
        public bool ToolPlatformProductInstanceUrl { get; set; }
        public bool ToolPlatformProductInstanceContactEmail { get; set; }
    }
}
