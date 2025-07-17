namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents a grade associated with a line item and user.
/// </summary>
public class Grade
{
    /// <summary>
    /// Gets or sets the identifier of the line item.
    /// </summary>
    public required string LineItemId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who scored the grade.
    /// </summary>
    public string? ScoringUserId { get; set; }

    /// <summary>
    /// Gets or sets the score of the grade.
    /// </summary>
    public decimal? ResultScore { get; set; }

    /// <summary>
    /// Gets or sets the maximum score of the grade.
    /// </summary>
    public decimal? ResultMaximum { get; set; }

    /// <summary>
    /// Gets or sets the comment associated with the grade.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the grade.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the release date and time of the grade.
    /// </summary>
    public DateTime? ReleaseDateTime { get; set; }

    /// <summary>
    /// Gets or sets the activity progress of the grade.
    /// </summary>
    public ActivityProgress ActivityProgress { get; set; }

    /// <summary>
    /// Gets or sets the grading progress of the grade.
    /// </summary>
    public GradingProgress GradingProgress { get; set; }

    /// <summary>
    /// Gets or sets the start date and time of the activity.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets the submission date and time of the activity.
    /// </summary>
    public DateTime? SubmittedAt { get; set; }
}

/// <summary>
/// Represents the progress of an activity.
/// </summary>
public enum ActivityProgress
{
    /// <summary>
    /// The activity has been initialized.
    /// </summary>
    Initialized,

    /// <summary>
    /// The activity has started.
    /// </summary>
    Started,

    /// <summary>
    /// The activity is in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// The activity has been submitted.
    /// </summary>
    Submitted,

    /// <summary>
    /// The activity has been completed.
    /// </summary>
    Completed
}

/// <summary>
/// Represents the progress of grading.
/// </summary>
public enum GradingProgress
{
    /// <summary>
    /// The grading is fully completed.
    /// </summary>
    FullyGraded,

    /// <summary>
    /// The grading is pending.
    /// </summary>
    Pending,

    /// <summary>
    /// The grading is pending manual intervention.
    /// </summary>
    PendingManual,

    /// <summary>
    /// The grading has failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The grading is not ready.
    /// </summary>
    NotReady
}
