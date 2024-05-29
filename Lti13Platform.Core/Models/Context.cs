namespace NP.Lti13Platform.Models
{
    public class Context
    {
        /// <summary>
        /// Max Length 255 characters
        /// Case sensitive
        /// </summary>
        public required string Id { get; set; }

        public required string DeploymentId { get; set; }

        public string? Label { get; set; }

        public string? Title { get; set; }

        public IEnumerable<string> Types { get; set; } = [];
    }
}
