namespace NP.Lti13Platform.Models
{
    public class Tool
    {
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
        public IEnumerable<string> Scopes { get; set; } = [];
    }

    public class CustomPermissions
    {

    }
}
