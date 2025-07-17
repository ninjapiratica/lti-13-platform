namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents an attempt on a resource link.
/// </summary>
public class Attempt
{
    /// <summary>
    /// Gets or sets the resource link ID.
    /// </summary>
    public required string ResourceLinkId { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// Gets or sets the available start date and time.
    /// </summary>
    public DateTime? AvailableStartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the available end date and time.
    /// </summary>
    public DateTime? AvailableEndDateTime { get; set; }

    /// <summary>
    /// Gets or sets the submission start date and time.
    /// </summary>
    public DateTime? SubmisstionStartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the submission end date and time.
    /// </summary>
    public DateTime? SubmissionEndDateTime { get; set; }
}
