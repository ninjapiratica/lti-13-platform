using NP.Lti13Platform.DeepLinking.Models;

namespace NP.Lti13Platform.DeepLinking;

/// <summary>
/// Represents a response from a deep linking tool as defined in the IMS Global LTI Deep Linking specification.
/// The Deep Linking Response Message is sent from the Tool to the Platform after the user has finished
/// selecting and/or configuring content in the Tool.
/// </summary>
public class DeepLinkResponse
{
    /// <summary>
    /// The opaque data value from the deep linking request. This value MUST be opaque to the Tool, and used as-is from the request, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// An optional plain text message that the Platform MAY display to the user as a result of the deep linking, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// An optional plain text message that the Platform MAY include in any logs or analytics, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public string? Log { get; set; }
    
    /// <summary>
    /// An optional plain text error message that the Platform MAY display to the user as a result of the deep linking, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// An optional plain text error message that the Platform MAY include in any logs or analytics, as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public string? ErrorLog { get; set; }

    /// <summary>
    /// An array of Content Items as defined in the IMS Global LTI Deep Linking specification. These items represent the content that the user has selected or created in the tool to be used within the Platform.
    /// </summary>
    public IEnumerable<ContentItem> ContentItems { get; set; } = [];
}