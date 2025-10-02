namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents the permissions for a user's personal data.
/// </summary>
public class UserPermissions
{
    /// <summary>
    /// The unique identifier for the user whose permissions are being represented, as defined by the LTI 1.3 and OpenID Connect Core specifications.
    /// </summary>
    public required string UserId { get; set; }
    /// <summary>
    /// Indicates whether the address claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool Address { get; set; }
    /// <summary>
    /// Indicates whether the country part of the address claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool AddressCountry { get; set; }
    /// <summary>
    /// Indicates whether the formatted address claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool AddressFormatted { get; set; }
    /// <summary>
    /// Indicates whether the locality part of the address claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool AddressLocality { get; set; }
    /// <summary>
    /// Indicates whether the postal code part of the address claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool AddressPostalCode { get; set; }
    /// <summary>
    /// Indicates whether the region part of the address claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool AddressRegion { get; set; }
    /// <summary>
    /// Indicates whether the street address part of the address claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool AddressStreetAddress { get; set; }
    /// <summary>
    /// Indicates whether the birthdate claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool Birthdate { get; set; }
    /// <summary>
    /// Indicates whether the email claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool Email { get; set; }
    /// <summary>
    /// Indicates whether the email verification status claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool EmailVerified { get; set; }
    /// <summary>
    /// Indicates whether the family name claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool FamilyName { get; set; }
    /// <summary>
    /// Indicates whether the gender claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool Gender { get; set; }
    /// <summary>
    /// Indicates whether the given name claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool GivenName { get; set; }
    /// <summary>
    /// Indicates whether the locale claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool Locale { get; set; }
    /// <summary>
    /// Indicates whether the middle name claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool MiddleName { get; set; }
    /// <summary>
    /// Indicates whether the full name claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool Name { get; set; }
    /// <summary>
    /// Indicates whether the nickname claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool Nickname { get; set; }
    /// <summary>
    /// Indicates whether the phone number claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool PhoneNumber { get; set; }
    /// <summary>
    /// Indicates whether the phone number verification status claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool PhoneNumberVerified { get; set; }
    /// <summary>
    /// Indicates whether the picture claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool Picture { get; set; }
    /// <summary>
    /// Indicates whether the preferred username claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool PreferredUsername { get; set; }
    /// <summary>
    /// Indicates whether the profile claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool Profile { get; set; }
    /// <summary>
    /// Indicates whether the last update timestamp claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool UpdatedAt { get; set; }
    /// <summary>
    /// Indicates whether the website claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool Website { get; set; }
    /// <summary>
    /// Indicates whether the timezone claim is accessible as defined by the OpenID Connect Core specification.
    /// </summary>
    public bool TimeZone { get; set; }
}
