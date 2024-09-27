using System.Text.Json.Serialization;

namespace NP.Lti13Platform.AssignmentGradeServices
{
    public interface IServiceEndpoints
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti-ags/claim/endpoint")]
        public LineItemServiceEndpoints? ServiceEndpoints { get; set; }

        public class LineItemServiceEndpoints
        {
            [JsonPropertyName("scope")]
            public required IEnumerable<string> Scopes { get; set; }

            [JsonPropertyName("lineitems")]
            public string? LineItemsUrl { get; set; }

            [JsonPropertyName("lineitem")]
            public string? LineItemUrl { get; set; }
        }
    }
}
