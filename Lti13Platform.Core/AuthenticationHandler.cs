using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core
{
    public class LtiMessage
    {
        [JsonPropertyName("iss")]
        public required string Issuer { get; set; }

        [JsonPropertyName("aud")]
        public required string Audience { get; set; }

        [JsonPropertyName("exp")]
        public long ExpirationDateUnix => new DateTimeOffset(IssuedDate).ToUnixTimeSeconds();

        [JsonIgnore]
        public DateTime ExpirationDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("iat")]
        public long IssuedDateUnix => new DateTimeOffset(IssuedDate).ToUnixTimeSeconds();

        [JsonIgnore]
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("nonce")]
        public required string Nonce { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/message_type")]
        public required string MessageType { get; set; }

        [JsonPropertyName("sub")]
        public string? Subject { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }

        [JsonPropertyName("middle_name")]
        public string? MiddleName { get; set; }

        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }

        [JsonPropertyName("preferred_username")]
        public string? PreferredUsername { get; set; }

        [JsonPropertyName("profile")]
        public string? Profile { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }

        [JsonPropertyName("website")]
        public string? Website { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("email_verified")]
        public bool? EmailVerified { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("birthdate")]
        public DateOnly? Birthdate { get; set; }

        [JsonPropertyName("zoneinfo")]
        public string? TimeZone { get; set; }

        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("phone_number_verified")]
        public bool? PhoneNumberVerified { get; set; }

        [JsonPropertyName("address")]
        public AddressClaim? Address { get; set; }

        [JsonIgnore]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public long? UpdatedAtUnix => !UpdatedAt.HasValue ? null : new DateTimeOffset(UpdatedAt.GetValueOrDefault()).ToUnixTimeSeconds();
    }

    public class AddressClaim
    {
        [JsonPropertyName("formatted")]
        public string? Formatted { get; set; }

        [JsonPropertyName("street_address")]
        public string? StreetAddress { get; set; }

        [JsonPropertyName("locality")]
        public string? Locality { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("postal_code")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }
    }
}

