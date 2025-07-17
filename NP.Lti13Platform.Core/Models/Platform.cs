namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents an LTI platform.
/// </summary>
public class Platform
{
    /// <summary>
    /// Gets or sets the GUID of the platform.
    /// </summary>
    public required string Guid { get; set; }

    /// <summary>
    /// Gets or sets the contact email of the platform.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Gets or sets the description of the platform.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the name of the platform.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the URL of the platform.
    /// </summary>
    public Uri? Url { get; set; }

    /// <summary>
    /// Gets or sets the product family code of the platform.
    /// </summary>
    public string? ProductFamilyCode { get; set; }

    /// <summary>
    /// Gets or sets the version of the platform.
    /// </summary>
    public string? Version { get; set; }
}
