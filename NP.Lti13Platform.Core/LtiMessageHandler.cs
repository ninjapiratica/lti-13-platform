using Microsoft.Extensions.Logging;
using NP.Lti13Platform.Core.Claims;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using System.Text.Json;

namespace NP.Lti13Platform.Core;

/// <summary>
/// Defines a contract for handling Learning Tools Interoperability (LTI) messages asynchronously and returning the result of the message processing operation.
/// </summary>
public interface ILtiMessageHandler
{
    /// <summary>
    /// Processes an LTI (Learning Tools Interoperability) message asynchronously and returns the result of the message handling operation.
    /// </summary>
    /// <param name="loginHint">A unique identifier provided by the platform to correlate the login request with the user. Cannot be null or empty.</param>
    /// <param name="ltiMessageHint">An optional hint provided by the platform to help identify the specific LTI message or context. May be null.</param>
    /// <param name="tool">The tool configuration used to validate and process the LTI message. Cannot be null.</param>
    /// <param name="nonce">A unique, random string used to prevent replay attacks. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an LtiMessageResult describing the outcome of the message handling.</returns>
    Task<LtiMessageResult> HandleLtiMessageAsync(string loginHint, string? ltiMessageHint, Tool tool, string nonce, CancellationToken cancellationToken = default);
}

internal class LtiResourceLinkMessageHandler(
    ILti13ResourceLinkMessageDataService dataService,
    ILti13TokenConfigService tokenConfigService,
    ILti13PlatformService platformService,
    ILogger<LtiResourceLinkMessageHandler> logger)
    : ILtiMessageHandler
{
    public static readonly string MessageType = "LtiResourceLinkRequest";

    private static bool TryDeserialize<T>(string jsonString, out T? deserilalized)
    {
        try
        {
            deserilalized = JsonSerializer.Deserialize<T>(jsonString);
            return true;
        }
        catch (JsonException)
        {
            deserilalized = default;
            return false;
        }
    }

    public async Task<LtiMessageResult> HandleLtiMessageAsync(
        string loginHint,
        string? ltiMessageHint,
        Tool tool,
        string nonce,
        CancellationToken cancellationToken = default)
    {
        if (loginHint.Split('|', 3, StringSplitOptions.TrimEntries) is not [var userIdString, var actualUserIdString, var isAnonymousString]
            || !bool.TryParse(isAnonymousString, out var isAnonymous))
        {
            return LtiMessageResult.None();
        }

        if (ltiMessageHint?.Split('|', 3, StringSplitOptions.TrimEntries) is not [var messageTypeString, var resourceLinkIdString, var launchPresentationOverrideString]
            || messageTypeString != MessageType
            || ResourceLinkId.TryParse(resourceLinkIdString, null, out var resourceLinkId)
            || TryDeserialize<LaunchPresentationOverride>(launchPresentationOverrideString, out var launchPresentationOverride))
        {
            return LtiMessageResult.None();
        }

        var resourceLink = await dataService.GetResourceLinkAsync(resourceLinkId, cancellationToken);
        if (resourceLink == null)
        {
            return LtiMessageResult.Error("");
        }

        var context = await dataService.GetContextAsync(resourceLink.ContextId, cancellationToken);
        if (context == null)
        {
            return LtiMessageResult.Error("");
        }

        var deployment = await dataService.GetDeploymentAsync(resourceLink.DeploymentId, cancellationToken);
        if (deployment == null || deployment.ClientId != tool.ClientId)
        {
            return LtiMessageResult.Error("");
        }

        User? user = null;
        if (UserId.TryParse(userIdString, null, out var userId)
            && userId != UserId.Empty)
        {
            user = await dataService.GetUserAsync(userId, cancellationToken);
            if (user == null)
            {
                return LtiMessageResult.Error("");
            }
        }

        User? actualUser = null;
        if (UserId.TryParse(actualUserIdString, null, out var actualUserId)
            && actualUserId != UserId.Empty)
        {
            actualUser = await dataService.GetUserAsync(actualUserId, cancellationToken);
            if (actualUser == null)
            {
                return LtiMessageResult.Error("");
            }
        }

        var tokenConfig = await tokenConfigService.GetTokenConfigAsync(tool.ClientId, cancellationToken);
        var platform = await platformService.GetPlatformAsync(tool.ClientId, cancellationToken);
        var customPermissions = await dataService.GetCustomPermissionsAsync(deployment.Id, context.Id, userId, actualUserId, cancellationToken);

        var lineItems = await dataService.GetLineItemsAsync(deployment.Id, context.Id, pageIndex: 0, limit: 1, resourceLinkId: resourceLink.Id, cancellationToken: cancellationToken);
        var lineItem = lineItems.TotalItems == 1 ? lineItems.Items.First() : null;
        var grade = lineItem != null && userId != UserId.Empty
            ? await dataService.GetGradeAsync(lineItem.Id, userId, cancellationToken)
            : null;

        var userPermissions = await dataService.GetUserPermissionsAsync(deployment.Id, context.Id, userId, cancellationToken);
        var userMembership = await dataService.GetMembershipAsync(context.Id, userId, cancellationToken);
        var attempt = await dataService.GetAttemptAsync(resourceLinkId, userId, cancellationToken);
        var actualUserMembership = await dataService.GetMembershipAsync(context.Id, actualUserId, cancellationToken);

        var ltiMessage = new LtiResourceLinkRequestMessage()
            .WithLtiMessageClaims(
                MessageType,
                nonce,
                tool.ClientId,
                tokenConfig)
            .WithLtiVersionClaims()
            .WithDeploymentIdClaims(deployment.Id)
            .WithTargetLinkUriClaims("")
            .WithResourceLinkClaims(resourceLink)
            .WithContextClaims(context)
            .WithCustomClaims(
                customPermissions,
                platform,
                tool,
                deployment,
                context,
                resourceLink,
                !isAnonymous ? userMembership : null,
                !isAnonymous ? user : null,
                !isAnonymous ? actualUserMembership : null,
                !isAnonymous ? actualUser : null,
                lineItem,
                attempt,
                grade);

        if (platform != null)
        {
            ltiMessage = ltiMessage
                .WithPlatformInstanceClaims(platform);
        }

        if (launchPresentationOverride != null)
        {
            ltiMessage = ltiMessage
               .WithLaunchPresentationClaims(launchPresentationOverride);
        }

        if (userMembership != null)
        {
            ltiMessage = ltiMessage
                .WithRolesClaims(userMembership, logger);

            if (!isAnonymous)
            {
                ltiMessage = ltiMessage
                    .WithRoleScopeMentorClaims(userMembership);
            }
        }

        if (!isAnonymous
            && userPermissions != null
            && user != null)
        {
            ltiMessage = ltiMessage
                .WithUserIdentityClaims(userPermissions, user);
        }


        return LtiMessageResult.Success(ltiMessage);
    }
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
    : ILtiMessage,
    ILtiVersionClaims,
    IDeploymentIdClaims,
    ILtiTargetLinkUriClaims,
    IResourceLinkClaims,
    IUserIdentityClaims,
    IRolesClaims,
    IContextClaims,
    IPlatformInstanceClaims,
    IRoleScopeMentorClaims,
    ILaunchPresentationClaims,
    ICustomClaims
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