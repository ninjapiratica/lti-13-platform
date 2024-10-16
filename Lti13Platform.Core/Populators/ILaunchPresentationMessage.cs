using System.Text.Json.Serialization;
using NP.Lti13Platform.Core.Constants;

namespace NP.Lti13Platform.Core.Populators
{
    public interface ILaunchPresentationMessage
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/launch_presentation")]
        public LaunchPresentationDefinition? LaunchPresentation { get; set; }

        public class LaunchPresentationDefinition
        {
            /// <summary>
            /// <see cref="Lti13PresentationTargetDocuments"/> has the list of possible values.
            /// </summary>
            [JsonPropertyName("document_target")]
            public string? DocumentTarget { get; set; }

            [JsonPropertyName("height")]
            public double? Height { get; set; }

            [JsonPropertyName("width")]
            public double? Width { get; set; }

            [JsonPropertyName("return_url")]
            public string? ReturnUrl { get; set; }

            [JsonPropertyName("locale")]
            public string? Locale { get; set; }
        }
    }

    public class LaunchPresentationOverride
    {
        /// <summary>
        /// <see cref="Lti13PresentationTargetDocuments"/> has the list of possible values.
        /// </summary>
        public string? DocumentTarget { get; set; }

        public double? Height { get; set; }

        public double? Width { get; set; }

        public string? ReturnUrl { get; set; }

        public string? Locale { get; set; }
    }
}
