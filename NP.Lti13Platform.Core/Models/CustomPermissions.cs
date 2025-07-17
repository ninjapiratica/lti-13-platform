namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents custom permissions for LTI claims.
/// </summary>
public class CustomPermissions
{
    /// <summary>
    /// Gets or sets a value indicating whether the user ID is accessible.
    /// </summary>
    public bool UserId { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the user image is accessible.
    /// </summary>
    public bool UserImage { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the user username is accessible.
    /// </summary>
    public bool UserUsername { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the user organization is accessible.
    /// </summary>
    public bool UserOrg { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the user scope mentor is accessible.
    /// </summary>
    public bool UserScopeMentor { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the user grade levels (OneRoster) are accessible.
    /// </summary>
    public bool UserGradeLevelsOneRoster { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the actual user ID is accessible.
    /// </summary>
    public bool ActualUserId { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the actual user image is accessible.
    /// </summary>
    public bool ActualUserImage { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the actual user username is accessible.
    /// </summary>
    public bool ActualUserUsername { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the actual user organization is accessible.
    /// </summary>
    public bool ActualUserOrg { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the actual user scope mentor is accessible.
    /// </summary>
    public bool ActualUserScopeMentor { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the actual user grade levels (OneRoster) are accessible.
    /// </summary>
    public bool ActualUserGradeLevelsOneRoster { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the context ID is accessible.
    /// </summary>
    public bool ContextId { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the context organization is accessible.
    /// </summary>
    public bool ContextOrg { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the context type is accessible.
    /// </summary>
    public bool ContextType { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the context label is accessible.
    /// </summary>
    public bool ContextLabel { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the context title is accessible.
    /// </summary>
    public bool ContextTitle { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the context sourced ID is accessible.
    /// </summary>
    public bool ContextSourcedId { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the context ID history is accessible.
    /// </summary>
    public bool ContextIdHistory { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the context grade levels (OneRoster) are accessible.
    /// </summary>
    public bool ContextGradeLevelsOneRoster { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the resource link ID is accessible.
    /// </summary>
    public bool ResourceLinkId { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the resource link title is accessible.
    /// </summary>
    public bool ResourceLinkTitle { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the resource link description is accessible.
    /// </summary>
    public bool ResourceLinkDescription { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the resource link available start date and time is accessible.
    /// </summary>
    public bool ResourceLinkAvailableStartDateTime { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the resource link available user start date and time is accessible.
    /// </summary>
    public bool ResourceLinkAvailableUserStartDateTime { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the resource link available end date and time is accessible.
    /// </summary>
    public bool ResourceLinkAvailableEndDateTime { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the resource link available user end date and time is accessible.
    /// </summary>
    public bool ResourceLinkAvailableUserEndDateTime { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the resource link submission start date and time is accessible.
    /// </summary>
    public bool ResourceLinkSubmissionStartDateTime { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the resource link submission user start date and time is accessible.
    /// </summary>
    public bool ResourceLinkSubmissionUserStartDateTime { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the resource link submission end date and time is accessible.
    /// </summary>
    public bool ResourceLinkSubmissionEndDateTime { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the resource link submission user end date and time is accessible.
    /// </summary>
    public bool ResourceLinkSubmissionUserEndDateTime { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the resource link line item release date and time is accessible.
    /// </summary>
    public bool ResourceLinkLineItemReleaseDateTime { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the resource link line item user release date and time is accessible.
    /// </summary>
    public bool ResourceLinkLineItemUserReleaseDateTime { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the resource link ID history is accessible.
    /// </summary>
    public bool ResourceLinkIdHistory { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tool platform product family code is accessible.
    /// </summary>
    public bool ToolPlatformProductFamilyCode { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the tool platform product version is accessible.
    /// </summary>
    public bool ToolPlatformProductVersion { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the tool platform product instance GUID is accessible.
    /// </summary>
    public bool ToolPlatformProductInstanceGuid { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the tool platform product instance name is accessible.
    /// </summary>
    public bool ToolPlatformProductInstanceName { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the tool platform product instance description is accessible.
    /// </summary>
    public bool ToolPlatformProductInstanceDescription { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the tool platform product instance URL is accessible.
    /// </summary>
    public bool ToolPlatformProductInstanceUrl { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the tool platform product instance contact email is accessible.
    /// </summary>
    public bool ToolPlatformProductInstanceContactEmail { get; set; }
}
