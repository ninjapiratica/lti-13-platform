namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents a line item in the gradebook as defined in the LTI 1.3 Assignment and Grade Services specification.
/// A line item represents a column in a gradebook that can be used for reporting scores or grades.
/// </summary>
public class LineItem
{
    /// <summary>
    /// Gets or sets the ID of the line item.
    /// A unique identifier for the line item as assigned by the platform. The line item ID should remain consistent across sessions and for the lifetime of the line item.
    /// </summary>
    public required LineItemId Id { get; set; }

    /// <summary>
    /// Gets or sets the deployment ID.
    /// Identifies the platform-tool integration that this line item is associated with. This value is provided by the platform when the tool is installed/registered.
    /// </summary>
    public required DeploymentId DeploymentId { get; set; }

    /// <summary>
    /// Gets or sets the context ID.
    /// The unique identifier of the context (course/section) with which this line item is associated. This links the line item to a specific context in the learning platform.
    /// </summary>
    public required ContextId ContextId { get; set; }

    /// <summary>
    /// Gets or sets the maximum score for the line item.
    /// Positive decimal value indicating the maximum score possible for this activity. Required as per the LTI Assignment and Grade Services specification.
    /// </summary>
    public required decimal ScoreMaximum { get; set; }

    /// <summary>
    /// Gets or sets the label for the line item.
    /// Label for the line item. This is the heading that will be shown in the gradebook. Required as per the LTI Assignment and Grade Services specification.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Gets or sets the resource link ID.
    /// The unique identifier of the resource link that originated this line item, if applicable. Can be null for line items not associated with a specific resource link.
    /// </summary>
    public ContentItemId? ResourceLinkId { get; set; }

    /// <summary>
    /// Gets or sets the resource ID.
    /// Tool provided ID for the resource. This can be used by the tool to identify or link to the specific activity or assessment.
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the tag for the line item.
    /// Additional information about the line item; may be used by the tool to identify line items attached to the same resource or resource link (example: grade, originality, participation).
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether grades have been released.
    /// Boolean to indicate if the platform should release the grades, e.g., to learners.
    /// </summary>
    public bool? GradesReleased { get; set; }

    /// <summary>
    /// Gets or sets the date and time when grades were released.
    /// The timestamp when the platform marked the grades for this line item as being released to students.
    /// </summary>
    public DateTime? GradesReleasedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the start date and time for the line item.
    /// Date and time when the line item becomes available for submission.
    /// </summary>
    public DateTime? StartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the end date and time for the line item.
    /// Date and time when the line item is no longer available for submission (i.e., the due date).
    /// </summary>
    public DateTime? EndDateTime { get; set; }
}

/// <summary>
/// Represents a unique identifier for a user.
/// </summary>
[StringId]
public readonly partial record struct LineItemId;
