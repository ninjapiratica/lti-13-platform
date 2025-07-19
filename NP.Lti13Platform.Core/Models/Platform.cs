namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents an LTI platform as defined in the LTI 1.3 core specification.
/// A platform (also known as a tool consumer) is a learning system that integrates external tools
/// using the LTI protocol, such as a Learning Management System (LMS).
/// </summary>
public class Platform
{
    /// <summary>
    /// Gets or sets the GUID of the platform.
    /// A globally unique identifier for the platform that should remain consistent across deployments.
    /// This is used in the 'iss' (Issuer) claim of LTI messages to identify the platform.
    /// </summary>
    public required string Guid { get; set; }

    /// <summary>
    /// Gets or sets the contact email of the platform.
    /// An email address that tool providers can use to contact the platform administrator.
    /// This is typically used for technical support or integration issues.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Gets or sets the description of the platform.
    /// A human-readable description of the platform that may be displayed to users or administrators.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the name of the platform.
    /// A human-readable name for the platform that is displayed to users.
    /// This should be recognizable to users of the system.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the URL of the platform.
    /// The base URL of the platform's web interface.
    /// This may be used for generating links back to the platform.
    /// </summary>
    public Uri? Url { get; set; }

    /// <summary>
    /// Gets or sets the product family code of the platform.
    /// An identifier for the product family that this platform instance belongs to.
    /// This can be used to identify the specific LMS product (e.g., "moodle", "canvas").
    /// </summary>
    public string? ProductFamilyCode { get; set; }

    /// <summary>
    /// Gets or sets the version of the platform.
    /// The version identifier for this platform instance.
    /// This typically includes the product version number of the LMS.
    /// </summary>
    public string? Version { get; set; }
}
