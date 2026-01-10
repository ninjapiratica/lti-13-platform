using NP.Lti13Platform.Core.MessageClaims;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.DeepLinking.MessageClaims;

namespace NP.Lti13Platform.DeepLinking.MessageHandlers;

/// <summary>
/// Represents an LTI Deep Linking request message as defined by the IMS LTI specification.
/// This interface exposes claims and properties relevant to deep linking launches, enabling tools to process and respond to deep linking requests from an LTI platform.
/// </summary>
/// <remarks>Implementations of this interface provide access to all standard claims required for LTI Deep Linking, including user identity, context, platform instance, and deep linking settings.
/// This interface is typically used by LTI tool providers to interpret and validate deep linking launch requests and to construct appropriate responses.
/// For more information about LTI Deep Linking, see the IMS Global LTI specification.</remarks>
public interface IDeepLinkingRequestMessage
    : IDeepLinkingSettingsClaims,
    ILti13Message,
    ILtiVersionClaims,
    IDeploymentIdClaims,
    IUserIdentityClaims,
    ILaunchPresentationClaims,
    IPlatformInstanceClaims,
    IContextClaims,
    IRolesClaims,
    IRoleScopeMentorClaims,
    ICustomClaims
{
}

/// <summary>
/// Represents an LTI 1.3 Deep Linking request message containing claims and user information required for deep linking between a tool and a platform.
/// </summary>
/// <remarks>This record encapsulates all claims defined by the LTI 1.3 Deep Linking specification, including user identity, context, platform, and deep linking settings.
/// It is typically used by LTI-compliant tools to process and respond to deep linking requests initiated by learning platforms.
/// All required and optional claims are exposed as properties to facilitate validation and handling of the deep linking workflow.</remarks>
public record DeepLinkingRequestMessage
    : IDeepLinkingRequestMessage
{
    /// <inheritdoc/>
    public IDeepLinkingSettingsClaims.DeepLinkingSettingsClaim DeepLinkSettings { get; set; } = null!;
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