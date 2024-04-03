namespace NP.Lti13Platform.Models
{
    public class Result
    {
        public Guid Id { get; set; }

        public Guid LineItemId { get; set; }

        public string UserId { get; set; }

        public string ScoringUserId { get; set; }

        public decimal ResultScore { get; set; }

        public decimal ResultMaximum { get; set; }

        public string? Comment { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
