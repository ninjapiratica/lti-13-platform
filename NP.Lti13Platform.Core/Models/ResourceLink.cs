namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents an LTI resource link as defined in the LTI 1.3 specification.
/// A resource link is a unique reference to a resource, from within a context in the tool consumer.
/// </summary>
public class ResourceLink
{
    /// <summary>
    /// Gets or sets the ID of the resource link.
    /// The stable unique identifier for the link as provided by the LMS platform. This value must be a UUID and must be immutable for a resource link.
    /// </summary>
    public required ResourceLinkId Id { get; set; }

    /// <summary>
    /// Gets or sets the deployment ID.
    /// Identifies the platform-tool integration governing the message. This value is provided by the platform when the tool is installed/registered.
    /// </summary>
    public required DeploymentId DeploymentId { get; set; }

    /// <summary>
    /// Gets or sets the context ID.
    /// An opaque identifier that uniquely identifies the context from which the resource link was launched.
    /// </summary>
    public required ContextId ContextId { get; set; }

    /// <summary>
    /// Gets or sets the URL of the resource link.
    /// The fully qualified URL of the resource that the link refers to.
    /// </summary>
    public Uri? Url { get; set; }

    /// <summary>
    /// String, plain text to use as the title or heading for content.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// String, plain text description of the content item intended to be displayed to all users who can access the item.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// date and time when the link becomes accessible.
    /// </summary>
    public DateTime? AvailableStartDateTime { get; set; }

    /// <summary>
    /// date and time when the link stops being accessible.
    /// </summary>
    public DateTime? AvailableEndDateTime { get; set; }

    /// <summary>
    /// Date and time when the link can start receiving submissions.
    /// </summary>
    public DateTime? SubmissionStartDateTime { get; set; }

    /// <summary>
    /// Date and time when the link stops accepting submissions.
    /// </summary>
    public DateTime? SubmissionEndDateTime { get; set; }

    /// <summary>
    /// The list of resource links that this resource link was copied from. Enables platforms to maintain continuity when a link is copied.
    /// </summary>
    public IEnumerable<string>? ClonedIdHistory { get; set; }

    /// <summary>
    /// A map of key/value custom parameters. Those parameters MUST be included in the LtiResourceLinkRequest payload. Value may include substitution parameters as defined in the LTI Core Specification. Map values must be strings. Note that "empty-string" is a valid value (""); however, null is not a valid value.
    /// </summary>
    public IDictionary<string, string>? Custom { get; set; }
}

/// <summary>
/// Represents a unique identifier for a user.
/// </summary>
[StringId]
public readonly partial record struct ResourceLinkId;