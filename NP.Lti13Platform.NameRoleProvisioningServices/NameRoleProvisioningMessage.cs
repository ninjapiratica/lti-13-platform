using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices;

public class NameRoleProvisioningMessage
{
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/message_type")]
    public required string MessageType { get; set; }
}
