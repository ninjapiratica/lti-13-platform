namespace NP.Lti13Platform
{
    public class Lti13OpenIdUser : ILti13Claim
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

            public IDictionary<string, object> GetClaims()
            {
                var dict = new Dictionary<string, object>();

                if (Formatted != null) dict.Add("formatted", Formatted);
                if (Street_Address != null) dict.Add("street_address", Street_Address);
                if (Locality != null) dict.Add("locality", Locality);
                if (Region != null) dict.Add("region", Region);
                if (Postal_Code != null) dict.Add("postal_code", Postal_Code);
                if (Country != null) dict.Add("country", Country);

                return dict;
            }
        }

        public IDictionary<string, object> GetClaims()
        {
            var dict = new Dictionary<string, object>();

            if (Sub != null) dict.Add("sub", Sub);
            if (Name != null) dict.Add("name", Name);
            if (Given_Name != null) dict.Add("given_name", Given_Name);
            if (Family_Name != null) dict.Add("family_name", Family_Name);
            if (Middle_Name != null) dict.Add("middle_name", Middle_Name);
            if (Nickname != null) dict.Add("nickname", Nickname);
            if (Preferred_Username != null) dict.Add("preferred_username", Preferred_Username);
            if (Profile != null) dict.Add("profile", Profile);
            if (Picture != null) dict.Add("picture", Picture);
            if (Website != null) dict.Add("website", Website);
            if (Email != null)
            {
                dict.Add("email", Email);
                if (Email_Verified != null) dict.Add("email_verified", Email_Verified);
            }
            if (Gender != null) dict.Add("gender", Gender);
            if (Birthdate != null) dict.Add("birthdate", Birthdate.Value.ToString("O"));
            if (ZoneInfo != null) dict.Add("zoneinfo", ZoneInfo);
            if (Locale != null) dict.Add("locale", Locale);
            if (Phone_Number != null)
            {
                dict.Add("phone_number", Phone_Number);
                if (Phone_Number_Verified != null) dict.Add("phone_number_verified", Phone_Number_Verified);
            }
            if (Updated_At != null) dict.Add("updated_at", Updated_At);

            var addressClaims = Address?.GetClaims();
            if (addressClaims?.Count > 0) dict.Add("address", addressClaims);

            return dict;
        }
    }
}
