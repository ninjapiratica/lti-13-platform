namespace NP.Lti13Platform.Core.Models
{
    public class Deployment
    {
        public required string Id { get; set; }

        public required string ToolId { get; set; }

        public IDictionary<string, string>? Custom { get; set; }
    }
}
