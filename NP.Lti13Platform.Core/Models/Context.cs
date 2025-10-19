namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents the context of an LTI platform as defined in the LTI 1.3 specification.
/// A context is a mapping to a group or collection of users in the tool consumer system, 
/// typically a course/class, project or other grouping of users.
/// </summary>
public class Context
{
    /// <summary>
    /// Gets or sets the unique identifier for the context.
    /// An opaque identifier that uniquely identifies the context that contains the link being launched. Max Length 255 characters. Case sensitive. This must be immutable for a context in the LMS platform.
    /// </summary>
    public required ContextId Id { get; set; }

    /// <summary>
    /// Gets or sets the label for the context.
    /// A plain text label for the context; intended to fit in a column. Example: "ECON 101" for an economics course.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the title of the context.
    /// A plain text title for the context; intended to be used as a descriptive title. Example: "Economics 101" for an economics course.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the sourced identifier for the context.
    /// An identifier for the context that is sourced from another system, such as a SIS. This identifier can be used to establish the relationship between a context in the tool consumer and the same context in an external system.
    /// </summary>
    public string? SourcedId { get; set; }

    /// <summary>
    /// Gets or sets the types associated with the context.
    /// An array of URNs that identify the type of context. These URNs correspond to values defined in the LTI specification. Common values include: 'course', 'group', 'course_offering', etc.
    /// </summary>
    public IEnumerable<string> Types { get; set; } = [];

    /// <summary>
    /// Gets or sets the organizations associated with the context.
    /// An array of organizational unit identifiers for the context. This can be used to relate the context to an organizational hierarchy.
    /// </summary>
    public IEnumerable<string> Orgs { get; set; } = [];

    /// <summary>
    /// Gets or sets the history of cloned identifiers for the context.
    /// The list of context IDs that this context was copied from. Enables platforms to maintain continuity when a context is copied or reused.
    /// </summary>
    public IEnumerable<ContextId> ClonedIdHistory { get; set; } = [];

    /// <summary>
    /// Gets or sets the OneRoster grades associated with the context.
    /// The grades represented by this context according to the OneRoster specification. Commonly used for K-12 contexts where grade levels are significant.
    /// </summary>
    public IEnumerable<string> OneRosterGrades { get; set; } = [];
}

/// <summary>
/// Represents a unique identifier for a <see cref="Context"/>.
/// </summary>
[StringId]
public readonly partial record struct ContextId;