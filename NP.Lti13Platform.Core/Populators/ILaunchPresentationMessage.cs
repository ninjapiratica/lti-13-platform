using System.Text.Json.Serialization;
using NP.Lti13Platform.Core.Constants;

namespace NP.Lti13Platform.Core.Populators;

/// <summary>
/// Defines the contract for an LTI launch presentation message.
/// </summary>
public interface ILaunchPresentationMessage
{
    /// <summary>
    /// Gets or sets the launch presentation information.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/launch_presentation")]
    public LaunchPresentationDefinition? LaunchPresentation { get; set; }

    /// <summary>
    /// Represents launch presentation configuration.
    /// </summary>
    public class LaunchPresentationDefinition
    {
        /// <summary>
        /// Gets or sets the document target.
        /// <see cref="Lti13PresentationTargetDocuments"/> has the list of possible values.
        /// </summary>
        [JsonPropertyName("document_target")]
        public string? DocumentTarget { get; set; }

        /// <summary>
        /// Gets or sets the height of the presentation window.
        /// </summary>
        [JsonPropertyName("height")]
        public double? Height { get; set; }

        /// <summary>
        /// Gets or sets the width of the presentation window.
        /// </summary>
        [JsonPropertyName("width")]
        public double? Width { get; set; }

        /// <summary>
        /// Gets or sets the return URL for the presentation.
        /// </summary>
        [JsonPropertyName("return_url")]
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// Gets or sets the locale for the presentation.
        /// </summary>
        [JsonPropertyName("locale")]
        public string? Locale { get; set; }
    }
}
