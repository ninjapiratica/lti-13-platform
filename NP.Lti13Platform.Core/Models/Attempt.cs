namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents an attempt on a resource link.
/// </summary>
public class Attempt
{
    /// <summary>
    /// The unique identifier of the resource link for which this attempt is made.
    /// </summary>
    public required ContentItemId ResourceLinkId { get; set; }

    /// <summary>
    /// The unique identifier of the user making the attempt.
    /// </summary>
    public required UserId UserId { get; set; }

    /// <summary>
    /// Date and time when the attempt becomes available.
    /// </summary>
    public DateTime? AvailableStartDateTime { get; set; }

    /// <summary>
    /// Date and time when the attempt is no longer available.
    /// </summary>
    public DateTime? AvailableEndDateTime { get; set; }

    /// <summary>
    /// Date and time when the user can start submitting for this attempt.
    /// </summary>
    public DateTime? SubmisstionStartDateTime { get; set; }

    /// <summary>
    /// Date and time when the user can no longer submit for this attempt.
    /// </summary>
    public DateTime? SubmissionEndDateTime { get; set; }
}
