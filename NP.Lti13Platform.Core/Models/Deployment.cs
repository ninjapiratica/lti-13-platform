namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents an LTI deployment.
/// </summary>
public class Deployment
{
    /// <summary>
    /// Gets or sets the ID of the deployment.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the tool ID.
    /// </summary>
    public required string ToolId { get; set; }

    /// <summary>
    /// Gets or sets the custom parameters for the deployment.
    /// </summary>
    public IDictionary<string, string>? Custom { get; set; }
}
