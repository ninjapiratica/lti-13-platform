namespace NP.Lti13Platform.Core.Models;

public class User
{
    public required string Id { get; set; }

    /// <summary>
    /// Full name in displayable form including all name parts, possibly including titles and suffixes, ordered according to the user's locale and preferences.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Given name(s) or first name(s). Note that in some cultures, people can have multiple given names; all can be present, with the names being separated by space characters.
    /// </summary>
    public string? GivenName { get; set; }

    /// <summary>
    /// Surname(s) or last name(s). Note that in some cultures, people can have multiple family names or no family name; all can be present, with the names being separated by space characters.
    /// </summary>
    public string? FamilyName { get; set; }

    /// <summary>
    /// Middle name(s). Note that in some cultures, people can have multiple middle names; all can be present, with the names being separated by space characters. Also note that in some cultures, middle names are not used.
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Casual name that may or may not be the same as the given_name. For instance, a nickname value of Mike might be returned alongside a given_name value of Michael.
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Shorthand name by which the user wishes to be referred to, such as janedoe or j.doe. This value MAY be any valid JSON string including special characters such as @, /, or whitespace. MUST NOT rely upon this value being unique.
    /// </summary>
    public string? PreferredUsername { get; set; }

    /// <summary>
    /// URL of the profile page. The contents of this Web page SHOULD be about the user.
    /// </summary>
    public Uri? Profile { get; set; }

    /// <summary>
    /// URL of the profile picture. This URL MUST refer to an image file (for example, a PNG, JPEG, or GIF image file), rather than to a Web page containing an image. Note that this URL SHOULD specifically reference a profile photo of the user suitable for displaying, rather than an arbitrary photo taken by the user.
    /// </summary>
    public Uri? Picture { get; set; }

    /// <summary>
    /// URL of the user's Web page or blog. This Web page SHOULD contain information published by the user or an organization that the user is affiliated with.
    /// </summary>
    public Uri? Website { get; set; }

    /// <summary>
    /// Preferred e-mail address. Its value MUST conform to the <see href="https://www.rfc-editor.org/rfc/rfc5322.txt">RFC 5322</see> addr-spec syntax. MUST NOT rely upon this value being unique.
    /// </summary>
    /// 
    public string? Email { get; set; }

    /// <summary>
    /// True if the user's e-mail address has been verified; otherwise false. When this value is true, this means that affirmative steps were taken to ensure that this e-mail address was controlled by the user at the time the verification was performed.
    /// </summary>
    public bool? EmailVerified { get; set; }

    /// <summary>
    /// Values defined by this specification are female and male. Other values MAY be used when neither of the defined values are applicable.
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// User's birthday, The year MAY be 0000, indicating that it is omitted.
    /// </summary>
    public DateOnly? Birthdate { get; set; }

    /// <summary>
    /// String from IANA Time Zone Database representing the user's time zone. For example, Europe/Paris or America/Los_Angeles.
    /// </summary>
    public string? TimeZone { get; set; }

    /// <summary>
    /// User's locale, represented as a <see href="https://www.rfc-editor.org/rfc/rfc5646.txt">BCP47</see> language tag. This is typically a language code in lowercase and an country code in uppercase, separated by a dash. For example, en-US or fr-CA. As a compatibility note, some implementations have used an underscore as the separator rather than a dash, for example, en_US; MAY choose to accept this locale syntax as well.
    /// </summary>
    public string? Locale { get; set; }

    /// <summary>
    /// User's preferred telephone number. <see href="https://www.itu.int/rec/T-REC-E.164-201011-I/en">E.164</see> is RECOMMENDED as the format, for example, +1 (425) 555-1212 or +56 (2) 687 2400. If the phone number contains an extension, it is RECOMMENDED that the extension be represented using the <see href="https://www.rfc-editor.org/rfc/rfc3966.txt">RFC 3966</see> extension syntax, for example, +1 (604) 555-1234;ext=5678.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// True if the user's phone number has been verified; otherwise false. When this is true, this means that the affirmative steps were taken to ensure that this phone number was controlled by the user at the time the verification was performed. When true, the phone_number Claim MUST be in E.164 format and any extensions MUST be represented in RFC 3966 format.
    /// </summary>
    public bool? PhoneNumberVerified { get; set; }

    /// <summary>
    /// Time the user's information was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User's preferred postal address.
    /// </summary>
    public Address? Address { get; set; }

    /// <summary>
    /// Username (typically, the name a user logs in with).
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// One or more URIs describing the user's organizational properties (for example, an ldap:// URI).
    /// </summary>
    public IEnumerable<Uri> Orgs { get; set; } = [];

    /// <summary>
    /// A list of grade(s) for which the user is enrolled. The permitted vocabulary is from the grades field utilized in <see href="https://ceds.ed.gov/CEDSElementDetails.aspx?TermId=7100">OneRoster Users</see>.
    /// </summary>
    public IEnumerable<string> OneRosterGrades { get; set; } = [];
}

public class Address
{
    /// <summary>
    /// Full mailing address, formatted for display or use on a mailing label. This field MAY contain multiple lines, separated by newlines. Newlines can be represented either as a carriage return/line feed pair ("\r\n") or as a single line feed character ("\n").
    /// </summary>
    public string? Formatted { get; set; }

    /// <summary>
    /// Full street address component, which MAY include house number, street name, Post Office Box, and multi-line extended street address information. This field MAY contain multiple lines, separated by newlines. Newlines can be represented either as a carriage return/line feed pair ("\r\n") or as a single line feed character ("\n").
    /// </summary>
    public string? StreetAddress { get; set; }

    /// <summary>
    /// City or locality component.
    /// </summary>
    public string? Locality { get; set; }

    /// <summary>
    /// State, province, prefecture, or region component.
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Zip code or postal code component.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country name component.
    /// </summary>
    public string? Country { get; set; }
}
