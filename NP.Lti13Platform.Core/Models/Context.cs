namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents the context of an LTI platform.
/// </summary>
public class Context
{
    /// <summary>
    /// Gets or sets the unique identifier for the context.
    /// Max Length 255 characters. Case sensitive.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the label for the context.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the title of the context.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the sourced identifier for the context.
    /// </summary>
    public string? SourcedId { get; set; }

    /// <summary>
    /// Gets or sets the types associated with the context.
    /// </summary>
    public IEnumerable<string> Types { get; set; } = [];

    /// <summary>
    /// Gets or sets the organizations associated with the context.
    /// </summary>
    public IEnumerable<string> Orgs { get; set; } = [];

    /// <summary>
    /// Gets or sets the history of cloned identifiers for the context.
    /// </summary>
    public IEnumerable<string> ClonedIdHistory { get; set; } = [];

    /// <summary>
    /// Gets or sets the OneRoster grades associated with the context.
    /// </summary>
    public IEnumerable<string> OneRosterGrades { get; set; } = [];
}
