using NP.Lti13Platform.DeepLinking.Models;

namespace NP.Lti13Platform.DeepLinking;

/// <summary>
/// Represents a response from a deep linking tool.
/// </summary>
public class DeepLinkResponse
{
    /// <summary>
    /// Gets or sets opaque data that was originally sent in the deep linking request.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Gets or sets a message to display to the user.
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Gets or sets log information for administrative purposes.
    /// </summary>
    public string? Log { get; set; }
    
    /// <summary>
    /// Gets or sets an error message to display to the user when an error occurs.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets error log information for administrative purposes.
    /// </summary>
    public string? ErrorLog { get; set; }

    /// <summary>
    /// Gets or sets the content items selected by the user.
    /// </summary>
    public IEnumerable<ContentItem> ContentItems { get; set; } = [];
}