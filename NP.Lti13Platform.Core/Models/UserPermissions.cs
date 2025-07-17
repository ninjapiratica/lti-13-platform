namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents the permissions for a user's personal data.
/// </summary>
public class UserPermissions
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the address is accessible.
    /// </summary>
    public bool Address { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the country part of the address is accessible.
    /// </summary>
    public bool AddressCountry { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the formatted address is accessible.
    /// </summary>
    public bool AddressFormatted { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the locality part of the address is accessible.
    /// </summary>
    public bool AddressLocality { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the postal code part of the address is accessible.
    /// </summary>
    public bool AddressPostalCode { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the region part of the address is accessible.
    /// </summary>
    public bool AddressRegion { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the street address part of the address is accessible.
    /// </summary>
    public bool AddressStreetAddress { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the birthdate is accessible.
    /// </summary>
    public bool Birthdate { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the email is accessible.
    /// </summary>
    public bool Email { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the email verification status is accessible.
    /// </summary>
    public bool EmailVerified { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the family name is accessible.
    /// </summary>
    public bool FamilyName { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the gender is accessible.
    /// </summary>
    public bool Gender { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the given name is accessible.
    /// </summary>
    public bool GivenName { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the locale is accessible.
    /// </summary>
    public bool Locale { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the middle name is accessible.
    /// </summary>
    public bool MiddleName { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the full name is accessible.
    /// </summary>
    public bool Name { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the nickname is accessible.
    /// </summary>
    public bool Nickname { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the phone number is accessible.
    /// </summary>
    public bool PhoneNumber { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the phone number verification status is accessible.
    /// </summary>
    public bool PhoneNumberVerified { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the picture is accessible.
    /// </summary>
    public bool Picture { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the preferred username is accessible.
    /// </summary>
    public bool PreferredUsername { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the profile is accessible.
    /// </summary>
    public bool Profile { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the last update timestamp is accessible.
    /// </summary>
    public bool UpdatedAt { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the website is accessible.
    /// </summary>
    public bool Website { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the timezone is accessible.
    /// </summary>
    public bool TimeZone { get; set; }
}
