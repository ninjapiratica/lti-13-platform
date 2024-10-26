namespace NP.Lti13Platform.Core.Models
{
    public class LineItem
    {
        public required string Id { get; set; }

        public required string DeploymentId { get; set; }

        public required string ContextId { get; set; }

        public required decimal ScoreMaximum { get; set; }

        public required string Label { get; set; }

        public string? ResourceLinkId { get; set; }

        public string? ResourceId { get; set; }

        public string? Tag { get; set; }

        public bool? GradesReleased { get; set; }

        public DateTime? GradesReleasedDateTime { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }
    }
}
