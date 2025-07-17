namespace NP.Lti13Platform.DeepLinking;

/// <summary>
/// Provides route names used internally by the LTI Deep Linking implementation.
/// </summary>
internal static class RouteNames
{
    /// <summary>
    /// Route name for the deep linking response endpoint.
    /// </summary>
    internal static readonly string DEEP_LINKING_RESPONSE = "DEEP_LINKING_RESPONSE";
}

/// <summary>
/// Provides constants for LTI 1.3 message types related to deep linking.
/// </summary>
public static class Lti13MessageType
{
    /// <summary>
    /// Represents a deep linking request message type.
    /// </summary>
    public static readonly string LtiDeepLinkingRequest = "LtiDeepLinkingRequest";
}

/// <summary>
/// Provides constants for content types that can be used in deep linking accept_types.
/// </summary>
public static class Lti13DeepLinkingTypes
{
    /// <summary>
    /// Represents a link content type.
    /// </summary>
    public static readonly string Link = "link";

    /// <summary>
    /// Represents a file content type.
    /// </summary>
    public static readonly string File = "file";

    /// <summary>
    /// Represents an HTML content type.
    /// </summary>
    public static readonly string Html = "html";

    /// <summary>
    /// Represents an LTI resource link content type.
    /// </summary>
    public static readonly string LtiResourceLink = "ltiResourceLink";

    /// <summary>
    /// Represents an image content type.
    /// </summary>
    public static readonly string Image = "image";
}
