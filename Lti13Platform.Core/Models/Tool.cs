namespace NP.Lti13Platform.Models
{
    public class Tool
    {
        public required string Id { get; set; }

        public required string ClientId { get; set; }

        public required string OidcInitiationUrl { get; set; }

        public required string DeepLinkUrl { get; set; }

        public required string LaunchUrl { get; set; }

        public IEnumerable<string> RedirectUrls => new[] { DeepLinkUrl, LaunchUrl }.Where(x => x != null).Select(x => x!);

        public Jwks? Jwks { get; set; }

        public IDictionary<string, string>? Custom { get; set; }

        public required UserPermissions UserPermissions { get; set; }

        public required ServicePermissions ServicePermissions { get; set; }

        public required CustomPermissions CustomPermissions { get; set; }
    }

    public class UserPermissions
    {
        public bool Address { get; set; }
        public bool AddressCountry { get; set; }
        public bool AddressFormatted { get; set; }
        public bool AddressLocality { get; set; }
        public bool AddressPostalCode { get; set; }
        public bool AddressRegion { get; set; }
        public bool AddressStreetAddress { get; set; }
        public bool Birthdate { get; set; }
        public bool Email { get; set; }
        public bool EmailVerified { get; set; }
        public bool FamilyName { get; set; }
        public bool Gender { get; set; }
        public bool GivenName { get; set; }
        public bool Locale { get; set; }
        public bool MiddleName { get; set; }
        public bool Name { get; set; }
        public bool Nickname { get; set; }
        public bool PhoneNumber { get; set; }
        public bool PhoneNumberVerified { get; set; }
        public bool Picture { get; set; }
        public bool PreferredUsername { get; set; }
        public bool Profile { get; set; }
        public bool UpdatedAt { get; set; }
        public bool Website { get; set; }
        public bool TimeZone { get; set; }
    }

    public class ServicePermissions
    {
        public IEnumerable<string> LineItemScopes { get; set; } = [];

        public bool AllowNameRoleProvisioningService { get; set; }
    }

    public class CustomPermissions
    {
        public bool UserId { get; set; }
        public bool UserImage { get; set; }
        public bool UserUsername { get; set; }
        public bool UserOrg { get; set; }
        public bool UserScopeMentor { get; set; }
        public bool UserGradeLevelsOneRoster { get; set; }

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
