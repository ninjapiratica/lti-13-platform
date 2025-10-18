namespace NP.Lti13Platform.DeepLinking;

/// <summary>
/// Route names need to be globally unique. To avoid the possibility of overlapping with other endpoints (outside of this library), the route names are made globally unique.
/// </summary>
internal static class RouteNames
{
    /// <summary>
    /// Route name for the deep linking response endpoint as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    internal static readonly string DEEP_LINKING_RESPONSE = "cca8466e-eb1f-4001-b290-0606c90f9f22";
}

/// <summary>
/// Provides constants for LTI 1.3 message types related to deep linking.
/// </summary>
public static class Lti13MessageType
{
    /// <summary>
    /// Represents a deep linking request message type.
    /// The LtiDeepLinkingRequest message type is used when the Platform requests that the Tool provide a deep linking experience to select or create content, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public static readonly string LtiDeepLinkingRequest = "LtiDeepLinkingRequest";
}

/// <summary>
/// Provides constants for content types that can be used in deep linking accept_types as defined
/// in the IMS Global LTI Deep Linking specification.
/// </summary>
public static class Lti13DeepLinkingTypes
{
    /// <summary>
    /// Represents a link content type.
    /// The link type provides a simple URL link to a resource hosted on the internet. This content type might be used to provide a link to a paper, or reference material, a text book companion site, or any resource that is accessed by clicking on a link, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public static readonly string Link = "link";

    /// <summary>
    /// Represents a file content type.
    /// The file type provides a URL link to a file hosted on the internet. This content type might be used when uploading a new file to the platform, for example, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public static readonly string File = "file";

    /// <summary>
    /// Represents an HTML content type.
    /// HTML Item allows display of HTML content. The content defined in the "html" property will be placed inside of an iframe and presented to the user, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public static readonly string Html = "html";

    /// <summary>
    /// Represents an LTI resource link content type.
    /// A link to an LTI resource, usually to be rendered within the same tool that provided the link, but when clicked, is a navigation from the platform to the tool, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public static readonly string LtiResourceLink = "ltiResourceLink";

    /// <summary>
    /// Represents an image content type.
    /// The image type provides a URL link to an image resource hosted on the internet. This content type might be used when providing access to an image file, for example, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public static readonly string Image = "image";
}
