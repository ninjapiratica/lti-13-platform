namespace NP.Lti13Platform.Core.Models;

public class Attempt
{
    public required string ResourceLinkId { get; set; }

    public required string UserId { get; set; }

    public DateTime? AvailableStartDateTime { get; set; }

    public DateTime? AvailableEndDateTime { get; set; }

    public DateTime? SubmisstionStartDateTime { get; set; } 

    public DateTime? SubmissionEndDateTime { get; set; }
}
