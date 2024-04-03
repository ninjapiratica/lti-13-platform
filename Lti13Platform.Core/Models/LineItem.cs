namespace NP.Lti13Platform.Models
{
    public class LineItem
    {
        public required Guid Id { get; set; }

        public required decimal ScoreMaximum { get; set; }

        public required string Label { get; set; }

        public Guid? ResourceLinkId { get; set; }

        public string? ResourceId { get; set; }

        public string? Tag { get; set; }

        public bool? GradesReleased { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }
    }
}
