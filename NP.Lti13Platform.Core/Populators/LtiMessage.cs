using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.Populators;

/// <summary>
/// Represents an LTI message that can be sent between a platform and tool.
/// This follows the JWT format as defined in the LTI 1.3 Core specification and includes
/// standard OpenID Connect claims along with LTI-specific claims.
/// </summary>
public class LtiMessage
{
    /// <summary>
    /// Gets or sets the issuer of the message.
    /// Issuer identifier of the platform instance initiating the launch.
    /// Required for all messages.
    /// </summary>
    [JsonPropertyName("iss")]
    public required string Issuer { get; set; }

    /// <summary>
    /// Gets or sets the audience of the message.
    /// OAuth 2.0 Client ID of the tool deployment that is the audience for this message.
    /// Required for all messages.
    /// </summary>
    [JsonPropertyName("aud")]
    public required string Audience { get; set; }

    /// <summary>
    /// Gets the expiration date as a Unix timestamp.
    /// Time at which the JWT MUST NOT be accepted for processing.
    /// Required for all messages.
    /// </summary>
    [JsonPropertyName("exp")]
    public long ExpirationDateUnix => new DateTimeOffset(ExpirationDate).ToUnixTimeSeconds();

    /// <summary>
    /// Gets or sets the expiration date of the message.
    /// </summary>
    [JsonIgnore]
    public DateTime ExpirationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the issued date as a Unix timestamp.
    /// Time at which the JWT was issued.
    /// Required for all messages.
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
    /// String value used to associate a Client session with an ID Token and to mitigate replay attacks.
    /// This is a unique value for each launch from a given issuer.
    /// Required for all messages.
    /// </summary>
    [JsonPropertyName("nonce")]
    public required string Nonce { get; set; }

    /// <summary>
    /// Gets or sets the message type.
    /// String indicating what type of LTI message is being sent.
    /// Required for all messages.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/message_type")]
    public required string MessageType { get; set; }

    /// <summary>
    /// Gets or sets the subject of the message.
    /// Locally unique and never reassigned identifier within the Issuer for the End-User.
    /// The platform MAY set this value as appropriate.
    /// </summary>
    [JsonPropertyName("sub")]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the user's full name.
    /// End-User's full name in displayable form including all name parts, possibly including titles and suffixes,
    /// ordered according to the End-User's locale and preferences.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the user's given name.
    /// Given name(s) or first name(s) of the End-User. Note that in some cultures, people can have multiple
    /// given names; all can be present, with the names being separated by space characters.
    /// </summary>
    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }

    /// <summary>
    /// Gets or sets the user's family name.
    /// Surname(s) or last name(s) of the End-User. Note that in some cultures, people can have multiple
    /// family names or no family name; all can be present, with the names being separated by space characters.
    /// </summary>
    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }

    /// <summary>
    /// Gets or sets the user's middle name.
    /// Middle name(s) of the End-User. Note that in some cultures, people can have multiple middle
    /// names; all can be present, with the names being separated by space characters. Also note that
    /// in some cultures, middle names are not used.
    /// </summary>
    [JsonPropertyName("middle_name")]
    public string? MiddleName { get; set; }

    /// <summary>
    /// Gets or sets the user's nickname.
    /// Casual name of the End-User that may or may not be the same as the given_name.
    /// For instance, a nickname value of Mike might be returned alongside a given_name value of Michael.
    /// </summary>
    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    /// <summary>
    /// Gets or sets the user's preferred username.
    /// Shorthand name by which the End-User wishes to be referred to at the RP, such as janedoe or j.doe.
    /// This value MAY be any valid JSON string including special characters such as @, /, or whitespace.
    /// </summary>
    [JsonPropertyName("preferred_username")]
    public string? PreferredUsername { get; set; }

    /// <summary>
    /// Gets or sets the URL to the user's profile.
    /// URL of the End-User's profile page. The contents of this Web page SHOULD be about the End-User.
    /// </summary>
    [JsonPropertyName("profile")]
    public string? Profile { get; set; }

    /// <summary>
    /// Gets or sets the URL to the user's picture.
    /// URL of the End-User's profile picture. This URL MUST refer to an image file (for example, a PNG,
    /// JPEG, or GIF image file), rather than to a Web page containing an image.
    /// </summary>
    [JsonPropertyName("picture")]
    public string? Picture { get; set; }

    /// <summary>
    /// Gets or sets the URL to the user's website.
    /// URL of the End-User's Web page or blog. This Web page SHOULD contain information published
    /// by the End-User or an organization that the End-User is affiliated with.
    /// </summary>
    [JsonPropertyName("website")]
    public string? Website { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// End-User's preferred e-mail address. Its value MUST conform to the RFC 5322 addr-spec syntax.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user's email address has been verified.
    /// True if the End-User's e-mail address has been verified; otherwise false.
    /// </summary>
    [JsonPropertyName("email_verified")]
    public bool? EmailVerified { get; set; }

    /// <summary>
    /// Gets or sets the user's gender.
    /// End-User's gender. Values defined by this specification are female and male.
    /// Other values MAY be used when neither of the defined values are applicable.
    /// </summary>
    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    /// <summary>
    /// Gets or sets the user's birthdate.
    /// End-User's birthday, represented as an ISO 8601:2004 YYYY-MM-DD format.
    /// The year MAY be 0000, indicating that it is omitted.
    /// </summary>
    [JsonPropertyName("birthdate")]
    public DateOnly? Birthdate { get; set; }

    /// <summary>
    /// Gets or sets the user's timezone.
    /// String from zoneinfo time zone database representing the End-User's time zone.
    /// For example, Europe/Paris or America/Los_Angeles.
    /// </summary>
    [JsonPropertyName("zoneinfo")]
    public string? TimeZone { get; set; }

    /// <summary>
    /// Gets or sets the user's locale.
    /// End-User's locale, represented as a BCP47 language tag. This is typically an
    /// ISO 639-1 Alpha-2 language code in lowercase and an ISO 3166-1 Alpha-2 country code in uppercase,
    /// separated by a dash. For example, en-US or fr-CA.
    /// </summary>
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    /// <summary>
    /// Gets or sets the user's phone number.
    /// End-User's preferred telephone number. E.164 is RECOMMENDED as the format,
    /// for example, +1 (425) 555-1212 or +56 (2) 687 2400.
    /// </summary>
    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user's phone number has been verified.
    /// True if the End-User's phone number has been verified; otherwise false.
    /// </summary>
    [JsonPropertyName("phone_number_verified")]
    public bool? PhoneNumberVerified { get; set; }

    /// <summary>
    /// Gets or sets the user's address information.
    /// End-User's preferred postal address as defined in OpenID Connect Core.
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
    /// Time the End-User's information was last updated.
    /// </summary>
    [JsonPropertyName("updated_at")]
    public long? UpdatedAtUnix => !UpdatedAt.HasValue ? null : new DateTimeOffset(UpdatedAt.GetValueOrDefault()).ToUnixTimeSeconds();
}

/// <summary>
/// Represents an address claim in an OpenID Connect message.
/// </summary>
public class AddressClaim
{
    /// <summary>
    /// Gets or sets the formatted address.
    /// Full mailing address, formatted for display or use on a mailing label.
    /// This field MAY contain multiple lines, separated by newlines.
    /// </summary>
    [JsonPropertyName("formatted")]
    public string? Formatted { get; set; }

    /// <summary>
    /// Gets or sets the street address.
    /// Full street address component, which MAY include house number, street name,
    /// Post Office Box, and multi-line extended street address information.
    /// </summary>
    [JsonPropertyName("street_address")]
    public string? StreetAddress { get; set; }

    /// <summary>
    /// Gets or sets the locality (city).
    /// City or locality component.
    /// </summary>
    [JsonPropertyName("locality")]
    public string? Locality { get; set; }

    /// <summary>
    /// Gets or sets the region (state).
    /// State, province, prefecture, or region component.
    /// </summary>
    [JsonPropertyName("region")]
    public string? Region { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// Zip code or postal code component.
    /// </summary>
    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the country.
    /// Country name component.
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }
}

