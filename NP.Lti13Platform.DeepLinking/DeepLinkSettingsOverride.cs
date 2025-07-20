namespace NP.Lti13Platform.DeepLinking;

/// <summary>
/// Represents override settings for deep linking functionality.
/// </summary>
public record DeepLinkSettingsOverride
{
    /// <summary>
    /// Gets or sets the URL where the platform should return the deep linking response.
    /// </summary>
    public string? DeepLinkReturnUrl { get; set; }

    /// <summary>
    /// Gets or sets the content types that are acceptable for deep linking.
    /// </summary>
    public IEnumerable<string>? AcceptTypes { get; set; }

    /// <summary>
    /// Gets or sets the document targets that are acceptable for presenting deep linked content.
    /// </summary>
    public IEnumerable<string>? AcceptPresentationDocumentTargets { get; set; }

    /// <summary>
    /// Gets or sets the media types that are acceptable for deep linking.
    /// </summary>
    public IEnumerable<string>? AcceptMediaTypes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether multiple content items can be selected.
    /// </summary>
    public bool? AcceptMultiple { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether line items can be accepted.
    /// </summary>
    public bool? AcceptLineItem { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether content items should be automatically created.
    /// </summary>
    public bool? AutoCreate { get; set; }

    /// <summary>
    /// Gets or sets the title to display for the deep linking interface.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the descriptive text to display for the deep linking interface.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets opaque data that will be passed back unchanged in the deep linking response.
    /// </summary>
    public string? Data { get; set; }
}
