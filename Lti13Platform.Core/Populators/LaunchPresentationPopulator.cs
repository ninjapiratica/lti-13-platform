using System.Text.Json.Serialization;

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

    public class LaunchPresentationPopulator : Populator<ILaunchPresentationMessage>
    {
        public override async Task Populate(ILaunchPresentationMessage obj, Lti13MessageScope scope)
        {
            // TODO: figureo out how to do this
            await Task.CompletedTask;
        }
    }
}
