namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents an LTI resource link.
/// </summary>
public class ResourceLink
{
    /// <summary>
    /// Gets or sets the ID of the resource link.
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
    /// Gets or sets the URL of the resource link.
    /// </summary>
    public Uri? Url { get; set; }

    /// <summary>
    /// Gets or sets the title of the resource link.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the text of the resource link.
    /// </summary>
    public string? Text { get; set; }

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
    public DateTime? SubmissionStartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the submission end date and time.
    /// </summary>
    public DateTime? SubmissionEndDateTime { get; set; }

    /// <summary>
    /// Gets or sets the history of cloned IDs.
    /// </summary>
    public IEnumerable<string>? ClonedIdHistory { get; set; }

    /// <summary>
    /// Gets or sets custom properties for the resource link.
    /// </summary>
    public IDictionary<string, string>? Custom { get; set; }
}
