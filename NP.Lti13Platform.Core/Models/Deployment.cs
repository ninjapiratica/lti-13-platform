namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents an LTI deployment.
/// </summary>
public class Deployment
{
    /// <summary>
    /// The unique identifier for the deployment as assigned by the platform. This value is used to identify the platform-tool integration governing the message.
    /// </summary>
    public required DeploymentId Id { get; set; }

    /// <summary>
    /// The unique identifier for the tool associated with this deployment.
    /// </summary>
    public required ClientId ClientId { get; set; }

    /// <summary>
    /// A map of key/value custom parameters for this deployment. These parameters MUST be included in LTI messages if present. Map values must be strings. Note that "empty-string" is a valid value (""); however, null is not a valid value.
    /// </summary>
    public IDictionary<string, string>? Custom { get; set; }
}

/// <summary>
/// Represents a unique identifier for a <see cref="Deployment"/>.
/// </summary>
[StringId]
public readonly partial record struct DeploymentId;