namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents an LTI platform as defined in the LTI 1.3 core specification.
/// A platform (also known as a tool consumer) is a learning system that integrates external tools
/// using the LTI protocol, such as a Learning Management System (LMS).
/// </summary>
public class Platform
{
    /// <summary>
    /// A stable locally unique to the iss identifier for an instance of the tool platform. The value of guid is a case-sensitive string that MUST NOT exceed 255 ASCII characters in length. The use of Universally Unique IDentifier (UUID) defined in [RFC4122] is recommended.
    /// </summary>
    public required string Guid { get; set; }

    /// <summary>
    /// Administrative contact email for the platform instance.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Descriptive phrase for the platform instance.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Name for the platform instance.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Home HTTPS URL endpoint for the platform instance.
    /// </summary>
    public Uri? Url { get; set; }

    /// <summary>
    /// Vendor product family code for the type of platform.
    /// </summary>
    public string? ProductFamilyCode { get; set; }

    /// <summary>
    /// Vendor product version for the platform.
    /// </summary>
    public string? Version { get; set; }
}
