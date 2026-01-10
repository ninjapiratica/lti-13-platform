using NP.Lti13Platform.Core.MessageClaims;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.MessageHandlers;

/// <summary>
/// Represents an LTI 1.3 resource link request message, providing access to claims and properties relevant to a resource link launch in a Learning Tools Interoperability (LTI) context.
/// </summary>
/// <remarks>This interface aggregates claims and properties from multiple LTI-related interfaces, including user identity, roles, context, platform instance, and custom claims.
/// It is typically used to model the data received during an LTI resource link launch, enabling tools to access launch parameters in a strongly typed manner.</remarks>
public interface ILtiResourceLinkRequestMessage
    : ILti13Message,
    ILtiVersionClaims,
    IDeploymentIdClaims,
    ITargetLinkUriClaims,
    IResourceLinkClaims,
    IUserIdentityClaims,
    IRolesClaims,
    IContextClaims,
    IPlatformInstanceClaims,
    IRoleScopeMentorClaims,
    ILaunchPresentationClaims,
    ICustomClaims
{
}

/// <summary>
/// Represents an LTI 1.3 resource link launch request message, containing claims and properties required for launching
/// a resource from a learning platform to a tool provider.
/// </summary>
/// <remarks>This record aggregates all standard and optional claims defined by the LTI 1.3 specification for a
/// resource link launch, including user identity, context, roles, platform instance, and custom claims. It is typically
/// used to deserialize and validate the payload of an LTI launch request received by a tool provider. All required
/// claims must be present and valid for a successful launch. Thread safety is not guaranteed for instances of this
/// type.</remarks>
public record LtiResourceLinkRequestMessage
    : ILtiResourceLinkRequestMessage
{
    /// <inheritdoc/>
    public string Issuer { get; set; } = string.Empty;
    /// <inheritdoc/>
    public string Audience { get; set; } = string.Empty;
    /// <inheritdoc/>
    public DateTime ExpirationDate { get; set; }
    /// <inheritdoc/>
    public DateTime IssuedDate { get; set; }
    /// <inheritdoc/>
    public string Nonce { get; set; } = string.Empty;
    /// <inheritdoc/>
    public string MessageType { get; set; } = string.Empty;
    /// <inheritdoc/>
    public string LtiVersion { get; set; } = string.Empty;
    /// <inheritdoc/>
    public DeploymentId DeploymentId { get; set; }
    /// <inheritdoc/>
    public string TargetLinkUri { get; set; } = string.Empty;
    /// <inheritdoc/>
    public IResourceLinkClaims.ResourceLinkClaim ResourceLink { get; set; } = null!;
    /// <inheritdoc/>
    public string? Subject { get; set; }
    /// <inheritdoc/>
    public string? Name { get; set; }
    /// <inheritdoc/>
    public string? GivenName { get; set; }
    /// <inheritdoc/>
    public string? FamilyName { get; set; }
    /// <inheritdoc/>
    public string? MiddleName { get; set; }
    /// <inheritdoc/>
    public string? Nickname { get; set; }
    /// <inheritdoc/>
    public string? PreferredUsername { get; set; }
    /// <inheritdoc/>
    public string? Profile { get; set; }
    /// <inheritdoc/>
    public string? Picture { get; set; }
    /// <inheritdoc/>
    public string? Website { get; set; }
    /// <inheritdoc/>
    public string? Email { get; set; }
    /// <inheritdoc/>
    public bool? EmailVerified { get; set; }
    /// <inheritdoc/>
    public string? Gender { get; set; }
    /// <inheritdoc/>
    public DateOnly? Birthdate { get; set; }
    /// <inheritdoc/>
    public string? TimeZone { get; set; }
    /// <inheritdoc/>
    public string? Locale { get; set; }
    /// <inheritdoc/>
    public string? PhoneNumber { get; set; }
    /// <inheritdoc/>
    public bool? PhoneNumberVerified { get; set; }
    /// <inheritdoc/>
    public AddressClaim? Address { get; set; }
    /// <inheritdoc/>
    public DateTime? UpdatedAt { get; set; }
    /// <inheritdoc/>
    public IEnumerable<string> Roles { get; set; } = [];
    /// <inheritdoc/>
    public IContextClaims.ContextClaim? Context { get; set; }
    /// <inheritdoc/>
    public IPlatformInstanceClaims.PlatformInstanceClaim? Platform { get; set; }
    /// <inheritdoc/>
    public IEnumerable<UserId>? RoleScopeMentor { get; set; }
    /// <inheritdoc/>
    public ILaunchPresentationClaims.LaunchPresentationClaim? LaunchPresentation { get; set; }
    /// <inheritdoc/>
    public IDictionary<string, string>? Custom { get; set; }
}