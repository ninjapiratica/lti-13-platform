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
    /// Gets or sets opaque data that was originally sent in the deep linking request.
    /// The Deep Linking Request Message included a "data" claim which contained the value that
    /// the Platform expects to be sent back in the response. This value MUST be opaque to the
    /// Tool, and used as-is from the request.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Gets or sets a message to display to the user.
    /// An optional plain text message that the Platform MAY display to the user as a result of the
    /// deep linking. For example, if the tool wishes to indicate that the content selection was
    /// successful, this parameter could have the value "Content selection successful!".
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Gets or sets log information for administrative purposes.
    /// An optional plain text message that the Platform MAY include in any logs or
    /// analytics. This can be useful for a tool to send tracking and analytics information
    /// regarding the content selection event without presenting this information to the end-user.
    /// </summary>
    public string? Log { get; set; }
    
    /// <summary>
    /// Gets or sets an error message to display to the user when an error occurs.
    /// An optional plain text error message that the Platform MAY display to the user as a result
    /// of the deep linking. For example, if the tool wishes to indicate that an error has occurred
    /// during the content selection process, this parameter could have the value
    /// "Error creating content".
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets error log information for administrative purposes.
    /// An optional plain text error message that the Platform MAY include in any logs or
    /// analytics. This can be useful for a Tool to send error information regarding the content
    /// selection event without presenting this information to the end-user.
    /// </summary>
    public string? ErrorLog { get; set; }

    /// <summary>
    /// Gets or sets the content items selected by the user.
    /// An array of Content Items as defined in the IMS Global LTI Deep Linking specification.
    /// These items represent the content that the user has selected or created in the tool
    /// to be used within the Platform.
    /// </summary>
    public IEnumerable<ContentItem> ContentItems { get; set; } = [];
}