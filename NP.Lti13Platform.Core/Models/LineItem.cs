namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents a line item in the gradebook as defined in the LTI 1.3 Assignment and Grade Services specification.
/// A line item represents a column in a gradebook that can be used for reporting scores or grades.
/// </summary>
public class LineItem
{
    /// <summary>
    /// Gets or sets the ID of the line item.
    /// A unique identifier for the line item as assigned by the platform.
    /// The line item ID should remain consistent across sessions and for the lifetime of the line item.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the deployment ID.
    /// Identifies the platform-tool integration that this line item is associated with.
    /// This value is provided by the platform when the tool is installed/registered.
    /// </summary>
    public required string DeploymentId { get; set; }

    /// <summary>
    /// Gets or sets the context ID.
    /// The unique identifier of the context (course/section) with which this line item is associated.
    /// This links the line item to a specific context in the learning platform.
    /// </summary>
    public required string ContextId { get; set; }

    /// <summary>
    /// Gets or sets the maximum score for the line item.
    /// The maximum possible score that can be achieved on this line item.
    /// Required as per the LTI Assignment and Grade Services specification.
    /// </summary>
    public required decimal ScoreMaximum { get; set; }

    /// <summary>
    /// Gets or sets the label for the line item.
    /// A human-readable label for the line item suitable for showing in gradebooks and other UIs.
    /// Required as per the LTI Assignment and Grade Services specification.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Gets or sets the resource link ID.
    /// The unique identifier of the resource link that originated this line item, if applicable.
    /// Can be null for line items not associated with a specific resource link.
    /// </summary>
    public string? ResourceLinkId { get; set; }

    /// <summary>
    /// Gets or sets the resource ID.
    /// A tool-specific identifier for the resource this line item represents.
    /// This can be used by the tool to identify or link to the specific activity or assessment.
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the tag for the line item.
    /// A tag that can be used to group or categorize line items.
    /// Tools can use this value to find line items with a common or shared purpose.
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether grades have been released.
    /// A boolean indicating whether the scores for this line item have been released to students.
    /// This enables tools to respect the platform's release policies.
    /// </summary>
    public bool? GradesReleased { get; set; }

    /// <summary>
    /// Gets or sets the date and time when grades were released.
    /// The timestamp when the platform marked the grades for this line item as being released to students.
    /// This can help tools understand when students gained visibility into their scores.
    /// </summary>
    public DateTime? GradesReleasedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the start date and time for the line item.
    /// The timestamp when this line item becomes available for submission.
    /// If not specified, the line item is available immediately.
    /// </summary>
    public DateTime? StartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the end date and time for the line item.
    /// The timestamp when this line item is no longer available for submission (i.e., the due date).
    /// If not specified, the line item can be submitted indefinitely.
    /// </summary>
    public DateTime? EndDateTime { get; set; }
}
