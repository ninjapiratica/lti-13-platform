namespace NP.Lti13Platform.Models
{
    public class Address
    {
        public required string Id { get; set; }

        public string? Formatted { get; set; }

        public string? StreetAddress { get; set; }
        
        public string? Locality { get; set; }

        public string? Region { get; set; }

        public string? PostalCode { get; set; }

        public string? Country { get; set; }
    }
}
