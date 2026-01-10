using NP.Lti13Platform.Core.Constants;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.MessageClaims;

/// <summary>
/// Defines the contract for an LTI 1.3 launch presentation message.
/// </summary>
public interface ILaunchPresentationClaims
{
    /// <summary>
    /// Gets or sets the launch presentation information.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/launch_presentation")]
    public LaunchPresentationClaim? LaunchPresentation { get; set; }

    /// <summary>
    /// Represents launch presentation configuration.
    /// </summary>
    public class LaunchPresentationClaim
    {
        /// <summary>
        /// Gets or sets the document target.
        /// <see cref="PresentationTargetDocuments"/> has the list of possible values.
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

public static partial class ClaimsExtensions
{
    /// <summary>
    /// Populates the launch presentation claims of the specified object using the provided override values.
    /// </summary>
    /// <remarks>This method is intended to simplify the process of assigning launch presentation claim values
    /// to objects that implement ILaunchPresentationClaims. The method overwrites any existing LaunchPresentation
    /// property values on the object.</remarks>
    /// <typeparam name="T">The type of the object implementing the ILaunchPresentationClaims interface.</typeparam>
    /// <param name="obj">The object whose LaunchPresentation property will be set.</param>
    /// <param name="launchPresentation">An object containing the override values for the launch presentation claims. Cannot be null.</param>
    /// <returns>The same object instance with its LaunchPresentation property set to the specified values.</returns>
    public static T WithLaunchPresentationClaims<T>(
        this T obj,
        LaunchPresentationOverride launchPresentation)
        where T : ILaunchPresentationClaims
    {
        obj.LaunchPresentation = new()
        {
            DocumentTarget = launchPresentation.DocumentTarget,
            Height = launchPresentation.Height,
            Locale = launchPresentation.Locale,
            ReturnUrl = launchPresentation.ReturnUrl,
            Width = launchPresentation.Width,
        };

        return obj;
    }
}
