using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.DeepLinking.Models;

/// <summary>
/// Represents a tool that supports deep linking functionality as defined in the LTI Deep Linking specification.
/// </summary>
/// <remarks>This type is used to configure and manage tools that provide deep linking capabilities,  allowing
/// platforms to launch the tool in deep linking mode for content selection.</remarks>
public record DeepLinkTool : Tool
{
    /// <summary>
    /// Gets or sets the deep link URL for the tool.
    /// The URL that the platform should use when launching the tool in deep linking mode. This is used for content selection as defined in the LTI Deep Linking specification.
    /// </summary>
    public required Uri DeepLinkUrl { get; set; }
}
