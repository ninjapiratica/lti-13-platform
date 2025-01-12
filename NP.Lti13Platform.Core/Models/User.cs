namespace NP.Lti13Platform.Core.Models;

public class User
{
    public required string Id { get; set; }

    public string? Name { get; set; }

    public string? GivenName { get; set; }

    public string? FamilyName { get; set; }

    public string? MiddleName { get; set; }

    public string? Nickname { get; set; }

    public string? PreferredUsername { get; set; }

    public string? Profile { get; set; }

    public string? Picture { get; set; }

    public string? Website { get; set; }

    public string? Email { get; set; }

    public bool? EmailVerified { get; set; }

    public string? Gender { get; set; }

    public DateOnly? Birthdate { get; set; }

    public string? TimeZone { get; set; }

    public string? Locale { get; set; }

    public string? PhoneNumber { get; set; }

    public bool? PhoneNumberVerified { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Address? Address { get; set; }

    public string? Username { get; set; }

    public IEnumerable<string> Orgs { get; set; } = [];

    public IEnumerable<string> OneRosterGrades { get; set; } = [];
}

public class Address
{
    public string? Formatted { get; set; }

    public string? StreetAddress { get; set; }

    public string? Locality { get; set; }

    public string? Region { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }
}
