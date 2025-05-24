using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core;

public record LtiBadRequest
{
    public required string Error { get; set; }

    [JsonPropertyName("error_description")]
    public required string Error_Description { get; set; }

    [JsonPropertyName("error_uri")]
    public required string Error_Uri { get; set; }
}
