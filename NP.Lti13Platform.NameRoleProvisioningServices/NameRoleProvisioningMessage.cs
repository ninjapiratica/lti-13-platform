using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices;

/// <summary>
/// Represents a message used in the Name and Role Provisioning Services for LTI 1.3.
/// </summary>
public class NameRoleProvisioningMessage
{
    /// <summary>
    /// Gets or sets the type of the message.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/message_type")]
    public required string MessageType { get; set; }
}
