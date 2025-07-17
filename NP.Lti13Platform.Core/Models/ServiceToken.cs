namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents a service token.
/// </summary>
public class ServiceToken
{
    /// <summary>
    /// Gets or sets the ID of the service token.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the tool ID.
    /// </summary>
    public required string ToolId { get; set; }

    /// <summary>
    /// Gets or sets the expiration date and time of the service token.
    /// </summary>
    public required DateTime Expiration { get; set; }
}
