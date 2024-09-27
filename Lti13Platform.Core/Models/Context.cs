namespace NP.Lti13Platform.Core.Models
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

        public string? SourcedId { get; set; }

        public IEnumerable<string> Types { get; set; } = [];

        public IEnumerable<string> Orgs { get; set; } = [];

        public IEnumerable<string> ClonedIdHistory { get; set; } = [];

        public IEnumerable<string> OneRosterGrades { get; set; } = [];
    }
}
