namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents a line item in the gradebook.
/// </summary>
public class LineItem
{
    /// <summary>
    /// Gets or sets the ID of the line item.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the deployment ID.
    /// </summary>
    public required string DeploymentId { get; set; }

    /// <summary>
    /// Gets or sets the context ID.
    /// </summary>
    public required string ContextId { get; set; }

    /// <summary>
    /// Gets or sets the maximum score for the line item.
    /// </summary>
    public required decimal ScoreMaximum { get; set; }

    /// <summary>
    /// Gets or sets the label for the line item.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Gets or sets the resource link ID.
    /// </summary>
    public string? ResourceLinkId { get; set; }

    /// <summary>
    /// Gets or sets the resource ID.
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the tag for the line item.
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether grades have been released.
    /// </summary>
    public bool? GradesReleased { get; set; }

    /// <summary>
    /// Gets or sets the date and time when grades were released.
    /// </summary>
    public DateTime? GradesReleasedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the start date and time for the line item.
    /// </summary>
    public DateTime? StartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the end date and time for the line item.
    /// </summary>
    public DateTime? EndDateTime { get; set; }
}
