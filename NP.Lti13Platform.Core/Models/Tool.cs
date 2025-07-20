namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents a tool in the LTI 1.3 platform as defined in the LTI 1.3 core specification.
/// A tool (also referred to as an LTI tool or tool provider) is an external application that can be 
/// integrated with a learning platform using the LTI protocol.
/// </summary>
public class Tool
{
    /// <summary>
    /// Gets or sets the unique identifier for the tool.
    /// A locally assigned identifier used by the platform to refer to this tool. This may differ from the client ID which is used for OAuth 2.0 authentication.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the client identifier for the tool.
    /// The OAuth 2.0 client identifier that the tool uses when making requests to the platform. This is a required value as per the LTI 1.3 specification and corresponds to the client_id parameter in OpenID Connect and OAuth 2.0.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the OIDC initiation URL for the tool.
    /// The URL to which the platform should send the OpenID Connect Authentication Request. This URL is used to initiate the LTI launch flow.
    /// </summary>
    public required Uri OidcInitiationUrl { get; set; }

    /// <summary>
    /// Gets or sets the deep link URL for the tool.
    /// The URL that the platform should use when launching the tool in deep linking mode. This is used for content selection as defined in the LTI Deep Linking specification.
    /// </summary>
    public required Uri DeepLinkUrl { get; set; }

    /// <summary>
    /// Gets or sets the launch URL for the tool.
    /// The URL that the platform should use when launching the tool for a standard LTI launch. This is the target of the OpenID Connect Authentication Response.
    /// </summary>
    public required Uri LaunchUrl { get; set; }

    /// <summary>
    /// Gets the redirect URLs for the tool.
    /// The collection of URLs that the platform may redirect to after initiating an LTI launch. As specified in the LTI 1.3 specification, these should include at minimum the deep linking URL and the launch URL.
    /// </summary>
    public IEnumerable<Uri> RedirectUrls => [DeepLinkUrl, LaunchUrl];

    /// <summary>
    /// Gets or sets the JSON Web Key Set (JWKS) for the tool.
    /// The cryptographic keys that the tool uses to sign messages. The platform uses these keys to verify the signature of messages from the tool.
    /// </summary>
    public Jwks? Jwks { get; set; }

    /// <summary>
    /// Gets or sets the custom parameters for the tool.
    /// A set of custom key-value pairs that should be included in LTI messages to this tool. These parameters are defined at the tool level and apply to all launches of the tool.
    /// </summary>
    public IDictionary<string, string>? Custom { get; set; }

    /// <summary>
    /// Gets or sets the service scopes for the tool.
    /// The collection of LTI Advantage service scopes that this tool is authorized to use. These define which LTI services (Assignment and Grade Services, Names and Roles, etc.) this tool is allowed to access.
    /// </summary>
    public IEnumerable<string> ServiceScopes { get; set; } = [];
}
