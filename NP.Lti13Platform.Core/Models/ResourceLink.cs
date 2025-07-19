namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents an LTI resource link as defined in the LTI 1.3 specification.
/// A resource link is a unique reference to a resource, from within a context in the tool consumer.
/// </summary>
public class ResourceLink
{
    /// <summary>
    /// Gets or sets the ID of the resource link.
    /// The stable unique identifier for the link as provided by the LMS platform.
    /// This value must be a UUID and must be immutable for a resource link.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the deployment ID.
    /// Identifies the platform-tool integration governing the message.
    /// This value is provided by the platform when the tool is installed/registered.
    /// </summary>
    public required string DeploymentId { get; set; }

    /// <summary>
    /// Gets or sets the context ID.
    /// An opaque identifier that uniquely identifies the context from which the resource link was launched.
    /// </summary>
    public required string ContextId { get; set; }

    /// <summary>
    /// Gets or sets the URL of the resource link.
    /// The fully qualified URL of the resource that the link refers to.
    /// </summary>
    public Uri? Url { get; set; }

    /// <summary>
    /// Gets or sets the title of the resource link.
    /// A plain text title for the resource, used for display purposes in the tool consumer.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the text of the resource link.
    /// A plain text description of the resource, used to enhance display in the tool consumer.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the available start date and time.
    /// The datetime when the resource becomes available for launch.
    /// If not specified, the resource is available for launch immediately.
    /// </summary>
    public DateTime? AvailableStartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the available end date and time.
    /// The datetime when the resource is no longer available for launch.
    /// If not specified, the resource is available indefinitely.
    /// </summary>
    public DateTime? AvailableEndDateTime { get; set; }

    /// <summary>
    /// Gets or sets the submission start date and time.
    /// The datetime when students can begin submitting to the resource.
    /// If not specified, students can submit as soon as the resource is available.
    /// </summary>
    public DateTime? SubmissionStartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the submission end date and time.
    /// The datetime when students can no longer submit to the resource (due date).
    /// If not specified, students can submit indefinitely.
    /// </summary>
    public DateTime? SubmissionEndDateTime { get; set; }

    /// <summary>
    /// Gets or sets the history of cloned IDs.
    /// The list of resource links that this resource link was copied from.
    /// This enables platforms to maintain continuity when a link is copied.
    /// </summary>
    public IEnumerable<string>? ClonedIdHistory { get; set; }

    /// <summary>
    /// Gets or sets custom properties for the resource link.
    /// A map of name/value custom parameters specific to this resource link.
    /// Names should not begin with "lti_" as these are reserved for LTI specification use.
    /// </summary>
    public IDictionary<string, string>? Custom { get; set; }
}
