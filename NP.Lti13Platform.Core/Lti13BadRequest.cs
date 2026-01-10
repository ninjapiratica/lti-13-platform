using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core;

/// <summary>
/// Represents an LTI 1.3 bad request error response.
/// </summary>
public record Lti13BadRequest
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public required string Error { get; set; }

    /// <summary>
    /// Gets or sets the error description.
    /// </summary>
    [JsonPropertyName("error_description")]
    public required string Error_Description { get; set; }

    /// <summary>
    /// Gets or sets the error URI for more information.
    /// </summary>
    [JsonPropertyName("error_uri")]
    public required string Error_Uri { get; set; }
}
