namespace NP.Lti13Platform.Models
{
    public class Deployment
    {
        public required string Id { get; set; }

        public required string ClientId { get; set; }

        public IDictionary<string, string>? Custom { get; set; }
    }
}
