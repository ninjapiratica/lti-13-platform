namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents a grade associated with a line item and user as defined in the LTI 1.3 Assignment and Grade Services specification.
/// A grade represents a score or evaluation result for a student on a specific line item.
/// </summary>
public class Grade
{
    /// <summary>
    /// Gets or sets the identifier of the line item.
    /// References the line item in the gradebook to which this grade belongs.
    /// </summary>
    public required string LineItemId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user.
    /// The user to whom this grade applies. This should correspond to a user ID in the platform.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who scored the grade.
    /// The user who created or last modified this grade (typically an instructor or grader).
    /// </summary>
    public string? ScoringUserId { get; set; }

    /// <summary>
    /// Gets or sets the score of the grade.
    /// The numeric score achieved by the student on the associated line item.
    /// This may be scaled by resultMaximum to determine the final grade percentage.
    /// </summary>
    public decimal? ResultScore { get; set; }

    /// <summary>
    /// Gets or sets the maximum score of the grade.
    /// The maximum possible score value for this result.
    /// If not specified, the scoreMaximum from the line item should be used.
    /// </summary>
    public decimal? ResultMaximum { get; set; }

    /// <summary>
    /// Gets or sets the comment associated with the grade.
    /// Optional instructor feedback or notes related to this grade.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the grade.
    /// The date and time when this grade was created or last modified.
    /// As specified by the ISO 8601 format in the LTI specification.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the release date and time of the grade.
    /// The date and time when this grade was or will be released to the student.
    /// Can be used for scheduled grade releases.
    /// </summary>
    public DateTime? ReleaseDateTime { get; set; }

    /// <summary>
    /// Gets or sets the activity progress of the grade.
    /// Indicates the status of the user's activity associated with this grade.
    /// As defined in the LTI Assignment and Grade Services specification.
    /// </summary>
    public ActivityProgress ActivityProgress { get; set; }

    /// <summary>
    /// Gets or sets the grading progress of the grade.
    /// Indicates the status of the grading process for this grade.
    /// As defined in the LTI Assignment and Grade Services specification.
    /// </summary>
    public GradingProgress GradingProgress { get; set; }

    /// <summary>
    /// Gets or sets the start date and time of the activity.
    /// When the user began working on the activity associated with this grade.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets the submission date and time of the activity.
    /// When the user submitted their work for the activity associated with this grade.
    /// </summary>
    public DateTime? SubmittedAt { get; set; }
}

/// <summary>
/// Represents the progress of an activity as defined in the LTI 1.3 Assignment and Grade Services specification.
/// This enumeration indicates the current state of a learner's activity.
/// </summary>
public enum ActivityProgress
{
    /// <summary>
    /// The activity has been initialized but not started.
    /// The tool consumer has created the activity but the user has not interacted with it yet.
    /// </summary>
    Initialized,

    /// <summary>
    /// The activity has been started.
    /// The user has begun the activity but has not yet completed a significant amount of work.
    /// </summary>
    Started,

    /// <summary>
    /// The activity is in progress.
    /// The user is actively working on the activity and has completed some portion of it.
    /// </summary>
    InProgress,

    /// <summary>
    /// The activity has been submitted.
    /// The user has submitted their work for the activity but it may not yet be considered complete.
    /// </summary>
    Submitted,

    /// <summary>
    /// The activity has been completed.
    /// The user has finished all required work for the activity.
    /// </summary>
    Completed
}

/// <summary>
/// Represents the progress of grading as defined in the LTI 1.3 Assignment and Grade Services specification.
/// This enumeration indicates the current state of the grading process.
/// </summary>
public enum GradingProgress
{
    /// <summary>
    /// The grading is fully completed.
    /// The submitted activity has been fully graded and the score represents the final value.
    /// </summary>
    FullyGraded,

    /// <summary>
    /// The grading is pending.
    /// The grading process has not yet begun.
    /// </summary>
    Pending,

    /// <summary>
    /// The grading is pending manual intervention.
    /// The grading process has been partially completed and requires manual review or intervention.
    /// </summary>
    PendingManual,

    /// <summary>
    /// The grading has failed.
    /// An error occurred during the grading process.
    /// </summary>
    Failed,

    /// <summary>
    /// The grading is not ready.
    /// The activity cannot be graded at this time, possibly because it is not yet complete.
    /// </summary>
    NotReady
}
