using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.Populators;

/// <summary>
/// Represents an LTI message that can be sent between a platform and tool.
/// </summary>
public class LtiMessage
{
    /// <summary>
    /// Gets or sets the issuer of the message.
    /// </summary>
    [JsonPropertyName("iss")]
    public required string Issuer { get; set; }

    /// <summary>
    /// Gets or sets the audience of the message.
    /// </summary>
    [JsonPropertyName("aud")]
    public required string Audience { get; set; }

    /// <summary>
    /// Gets the expiration date as a Unix timestamp.
    /// </summary>
    [JsonPropertyName("exp")]
    public long ExpirationDateUnix => new DateTimeOffset(IssuedDate).ToUnixTimeSeconds();

    /// <summary>
    /// Gets or sets the expiration date of the message.
    /// </summary>
    [JsonIgnore]
    public DateTime ExpirationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the issued date as a Unix timestamp.
    /// </summary>
    [JsonPropertyName("iat")]
    public long IssuedDateUnix => new DateTimeOffset(IssuedDate).ToUnixTimeSeconds();

    /// <summary>
    /// Gets or sets the issued date of the message.
    /// </summary>
    [JsonIgnore]
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the nonce of the message.
    /// </summary>
    [JsonPropertyName("nonce")]
    public required string Nonce { get; set; }

    /// <summary>
    /// Gets or sets the message type.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/message_type")]
    public required string MessageType { get; set; }

    /// <summary>
    /// Gets or sets the subject of the message.
    /// </summary>
    [JsonPropertyName("sub")]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the user's full name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the user's given name.
    /// </summary>
    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }

    /// <summary>
    /// Gets or sets the user's family name.
    /// </summary>
    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }

    /// <summary>
    /// Gets or sets the user's middle name.
    /// </summary>
    [JsonPropertyName("middle_name")]
    public string? MiddleName { get; set; }

    /// <summary>
    /// Gets or sets the user's nickname.
    /// </summary>
    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    /// <summary>
    /// Gets or sets the user's preferred username.
    /// </summary>
    [JsonPropertyName("preferred_username")]
    public string? PreferredUsername { get; set; }

    /// <summary>
    /// Gets or sets the URL to the user's profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public string? Profile { get; set; }

    /// <summary>
    /// Gets or sets the URL to the user's picture.
    /// </summary>
    [JsonPropertyName("picture")]
    public string? Picture { get; set; }

    /// <summary>
    /// Gets or sets the URL to the user's website.
    /// </summary>
    [JsonPropertyName("website")]
    public string? Website { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user's email address has been verified.
    /// </summary>
    [JsonPropertyName("email_verified")]
    public bool? EmailVerified { get; set; }

    /// <summary>
    /// Gets or sets the user's gender.
    /// </summary>
    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    /// <summary>
    /// Gets or sets the user's birthdate.
    /// </summary>
    [JsonPropertyName("birthdate")]
    public DateOnly? Birthdate { get; set; }

    /// <summary>
    /// Gets or sets the user's timezone.
    /// </summary>
    [JsonPropertyName("zoneinfo")]
    public string? TimeZone { get; set; }

    /// <summary>
    /// Gets or sets the user's locale.
    /// </summary>
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    /// <summary>
    /// Gets or sets the user's phone number.
    /// </summary>
    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user's phone number has been verified.
    /// </summary>
    [JsonPropertyName("phone_number_verified")]
    public bool? PhoneNumberVerified { get; set; }

    /// <summary>
    /// Gets or sets the user's address information.
    /// </summary>
    [JsonPropertyName("address")]
    public AddressClaim? Address { get; set; }

    /// <summary>
    /// Gets or sets the time when the user's information was last updated.
    /// </summary>
    [JsonIgnore]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets the time when the user's information was last updated as a Unix timestamp.
    /// </summary>
    [JsonPropertyName("updated_at")]
    public long? UpdatedAtUnix => !UpdatedAt.HasValue ? null : new DateTimeOffset(UpdatedAt.GetValueOrDefault()).ToUnixTimeSeconds();
}

/// <summary>
/// Represents an address claim in an LTI message.
/// </summary>
public class AddressClaim
{
    /// <summary>
    /// Gets or sets the formatted address.
    /// </summary>
    [JsonPropertyName("formatted")]
    public string? Formatted { get; set; }

    /// <summary>
    /// Gets or sets the street address.
    /// </summary>
    [JsonPropertyName("street_address")]
    public string? StreetAddress { get; set; }

    /// <summary>
    /// Gets or sets the locality (city).
    /// </summary>
    [JsonPropertyName("locality")]
    public string? Locality { get; set; }

    /// <summary>
    /// Gets or sets the region (state).
    /// </summary>
    [JsonPropertyName("region")]
    public string? Region { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }
}

