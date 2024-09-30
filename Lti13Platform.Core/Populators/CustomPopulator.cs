using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.Populators
{
    public interface ICustomMessage
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/custom")]
        public IDictionary<string, string>? Custom { get; set; }
    }

    public class CustomPopulator(CustomReplacements customReplacements) : Populator<ICustomMessage>
    {
        public override async Task Populate(ICustomMessage obj, Lti13MessageScope scope)
        {
            obj.Custom = await customReplacements.ReplaceAsync(scope);
        }
    }
}
