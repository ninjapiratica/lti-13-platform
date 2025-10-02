namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents a service token.
/// </summary>
public class ServiceToken
{
    /// <summary>
    /// The unique identifier for the service token as defined by the LTI 1.3 specification.
    /// </summary>
    public required ServiceTokenId Id { get; set; }

    /// <summary>
    /// The unique identifier for the tool associated with this service token as defined by the LTI 1.3 specification.
    /// </summary>
    public required ClientId ClientId { get; set; }

    /// <summary>
    /// The expiration date and time of the service token as defined by the LTI 1.3 specification.
    /// </summary>
    public required DateTime Expiration { get; set; }
}

/// <summary>
/// Represents a unique identifier for a user.
/// </summary>
[StringId]
public readonly partial record struct ServiceTokenId;
