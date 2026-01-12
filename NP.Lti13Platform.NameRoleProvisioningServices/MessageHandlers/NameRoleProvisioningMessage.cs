using NP.Lti13Platform.Core.MessageClaims;
using NP.Lti13Platform.NameRoleProvisioningServices.MessageClaims;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices.MessageHandlers;

/// <summary>
/// Represents a message used in the Name and Role Provisioning Services for LTI 1.3.
/// </summary>
public interface INameRoleProvisioningMessage
    : IBaseLti13Message,
    INrpsCustomClaims
{
    /// <summary>
    /// Gets or sets the type of the message.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/message_type")]
    string MessageType { get; set; }
}

/// <summary>
/// Represents a message used in the Name and Role Provisioning Services for LTI 1.3.
/// </summary>
internal class NameRoleProvisioningLtiResourceLinkMessage
    : INameRoleProvisioningMessage
{
    public IDictionary<string, string>? Custom { get; set; }

    public string MessageType { get; set; } = "LtiResourceLinkRequest";
}