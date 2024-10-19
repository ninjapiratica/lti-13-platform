namespace NP.Lti13Platform.Core.Models
{
    public class Grade
    {
        public required string LineItemId { get; set; }

        public required string UserId { get; set; }

        public string? ScoringUserId { get; set; }

        public decimal? ResultScore { get; set; }

        public decimal? ResultMaximum { get; set; }

        public string? Comment { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime? ReleaseDateTime { get; set; }

        public ActivityProgress ActivityProgress { get; set; }

        public GradingProgress GradingProgress { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }
    }

    public enum ActivityProgress
    {
        Initialized,
        Started,
        InProgress,
        Submitted,
        Completed
    }
    public enum GradingProgress
    {
        FullyGraded,
        Pending,
        PendingManual,
        Failed,
        NotReady
    }
}
