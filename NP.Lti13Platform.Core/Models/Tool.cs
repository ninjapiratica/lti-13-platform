namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents a tool in the LTI 1.3 platform.
/// </summary>
public class Tool
{
    /// <summary>
    /// Gets or sets the unique identifier for the tool.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the client identifier for the tool.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the OIDC initiation URL for the tool.
    /// </summary>
    public required Uri OidcInitiationUrl { get; set; }

    /// <summary>
    /// Gets or sets the deep link URL for the tool.
    /// </summary>
    public required Uri DeepLinkUrl { get; set; }

    /// <summary>
    /// Gets or sets the launch URL for the tool.
    /// </summary>
    public required Uri LaunchUrl { get; set; }

    /// <summary>
    /// Gets the redirect URLs for the tool.
    /// </summary>
    public IEnumerable<Uri> RedirectUrls => [DeepLinkUrl, LaunchUrl];

    /// <summary>
    /// Gets or sets the JSON Web Key Set (JWKS) for the tool.
    /// </summary>
    public Jwks? Jwks { get; set; }

    /// <summary>
    /// Gets or sets the custom parameters for the tool.
    /// </summary>
    public IDictionary<string, string>? Custom { get; set; }

    /// <summary>
    /// Gets or sets the service scopes for the tool.
    /// </summary>
    public IEnumerable<string> ServiceScopes { get; set; } = [];
}
