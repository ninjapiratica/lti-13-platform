namespace NP.Lti13Platform.Models
{
    public class Result
    {
        public required string LineItemId { get; set; }

        public required string UserId { get; set; }

        public string? ScoringUserId { get; set; }

        public decimal ResultScore { get; set; }

        public decimal ResultMaximum { get; set; }

        public string? Comment { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
