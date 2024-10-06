namespace NP.Lti13Platform.Core.Models
{
    public class ServiceToken
    {
        public required string Id { get; set; }

        public required string ToolId { get; set; }

        public required DateTime Expiration { get; set; }
    }
}
