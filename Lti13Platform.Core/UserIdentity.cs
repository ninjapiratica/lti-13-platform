using System.Security.Claims;
using System.Text.Json;

namespace NP.Lti13Platform.Core
{
    public class UserIdentity
    {
        public string? Sub { get; set; }
        public string? Name { get; set; }
        public string? Given_Name { get; set; }
        public string? Family_Name { get; set; }
        public string? Middle_Name { get; set; }
        public string? Nickname { get; set; }
        public string? Preferred_Username { get; set; }
        public string? Profile { get; set; }
        public string? Picture { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }
        public bool? Email_Verified { get; set; }
        public string? Gender { get; set; }
        public DateOnly? Birthdate { get; set; }
        public string? ZoneInfo { get; set; }
        public string? Locale { get; set; }
        public string? Phone_Number { get; set; }
        public bool? Phone_Number_Verified { get; set; }
        public AddressClaim? Address { get; set; }
        public string? Updated_At { get; set; } // Convert to UNIX timestamp

        public class AddressClaim
        {
            public string? Formatted { get; set; }
            public string? Street_Address { get; set; }
            public string? Locality { get; set; }
            public string? Region { get; set; }
            public string? Postal_Code { get; set; }
            public string? Country { get; set; }

            public override string ToString()
            {
                var dict = new Dictionary<string, string>();

                if (Formatted != null) dict.Add("formatted", Formatted);
                if (Street_Address != null) dict.Add("street_address", Street_Address);
                if (Locality != null) dict.Add("locality", Locality);
                if (Region != null) dict.Add("region", Region);
                if (Postal_Code != null) dict.Add("postal_code", Postal_Code);
                if (Country != null) dict.Add("country", Country);

                return JsonSerializer.Serialize(dict);
            }
        }

        public async IAsyncEnumerable<Claim> GetClaimsAsync()
        {
            await LoadUserAsync();

            if (Sub != null) yield return new Claim("sub", Sub);
            if (Name != null) yield return new Claim("name", Name);
            if (Given_Name != null) yield return new Claim("given_name", Given_Name);
            if (Family_Name != null) yield return new Claim("family_name", Family_Name);
            if (Middle_Name != null) yield return new Claim("middle_name", Middle_Name);
            if (Nickname != null) yield return new Claim("nickname", Nickname);
            if (Preferred_Username != null) yield return new Claim("preferred_username", Preferred_Username);
            if (Profile != null) yield return new Claim("profile", Profile);
            if (Picture != null) yield return new Claim("picture", Picture);
            if (Website != null) yield return new Claim("website", Website);
            if (Email != null)
            {
                yield return new Claim("email", Email);
                if (Email_Verified != null) yield return new Claim("email_verified", Email_Verified.Value.ToString(), ClaimValueTypes.Boolean);
            }
            if (Gender != null) yield return new Claim("gender", Gender);
            if (Birthdate != null) yield return new Claim("birthdate", Birthdate.Value.ToString("O"));
            if (ZoneInfo != null) yield return new Claim("zoneinfo", ZoneInfo);
            if (Locale != null) yield return new Claim("locale", Locale);
            if (Phone_Number != null)
            {
                yield return new Claim("phone_number", Phone_Number);
                if (Phone_Number_Verified != null) yield return new Claim("phone_number_verified", Phone_Number_Verified.Value.ToString(), ClaimValueTypes.Boolean);
            }
            //if (Address != null) yield return new Claim("address", Address.ToString(), JsonClaimValueTypes.Json);
            if (Updated_At != null) yield return new Claim("updated_at", Updated_At);
        }

        public virtual Task LoadUserAsync() => Task.CompletedTask;
    }
}
