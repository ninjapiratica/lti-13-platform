using Microsoft.Extensions.Logging;
using NP.Lti13Platform.Core.Extensions;
using NP.Lti13Platform.Core.MessageClaims;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using System.Text.Json;

namespace NP.Lti13Platform.Core.MessageHandlers;

/// <summary>
/// Defines methods for handling LTI resource link launch requests, enabling retrieval and creation of LTI launch information for specific resource links and user contexts.
/// </summary>
/// <remarks>Implementations of this interface provide functionality for constructing and retrieving LTI launch data in accordance with the LTI specification.
/// Methods support scenarios such as anonymous launches, acting on behalf of another user, and customizing launch presentation parameters.
/// This interface extends ILtiMessageHandler to support resource link-specific launch workflows.</remarks>
public interface ILti13ResourceLinkRequestMessageHandler : ILtiMessageHandler
{
    /// <summary>
    /// Asynchronously retrieves the LTI launch information for the specified resource link and user context.
    /// </summary>
    /// <param name="resourceLinkId">The identifier of the LTI resource link for which to retrieve launch information.</param>
    /// <param name="userId">The identifier of the user on whose behalf the launch is being performed. If null, the launch is not associated with a specific user.</param>
    /// <param name="actualUserId">The identifier of the actual user performing the launch, if different from <paramref name="userId"/>.
    /// Used in scenarios such as acting on behalf of another user. If null, defaults to the value of <paramref name="userId"/>.</param>
    /// <param name="isAnonymous">true if the launch should be performed without associating it with a user identity; otherwise, false.</param>
    /// <param name="launchPresentationOverride">An optional override for launch presentation parameters, such as display target or return URL. If null, default presentation settings are used.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the LTI launch information for the specified resource link and user context.</returns>
    Task<LtiLaunch?> GetLtiLaunchAsync(ResourceLinkId resourceLinkId, UserId? userId = null, UserId? actualUserId = null, bool isAnonymous = false, LaunchPresentationOverride? launchPresentationOverride = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an LTI launch request for the specified issuer, tool, and resource link, with optional user and presentation overrides.
    /// </summary>
    /// <remarks>If isAnonymous is set to true, user information will be omitted from the launch request regardless of the userId provided.
    /// Use launchPresentationOverride to customize how the tool is presented to the user during the launch.</remarks>
    /// <param name="issuer">The URI identifying the platform (issuer) initiating the LTI launch. Cannot be null.</param>
    /// <param name="tool">The tool configuration to use for the launch. Cannot be null.</param>
    /// <param name="resourceLink">The resource link representing the context or activity being launched. Cannot be null.</param>
    /// <param name="userId">The user identifier to associate with the launch. If null, the launch will not be associated with a specific user unless required by the tool.</param>
    /// <param name="actualUserId">The actual user identifier if the launch is being performed on behalf of another user. If null, the launch is performed as the user specified by userId.</param>
    /// <param name="isAnonymous">true if the launch should be performed without identifying the user; otherwise, false.</param>
    /// <param name="launchPresentationOverride">An optional override for launch presentation parameters, such as display or window preferences. If null, default presentation settings are used.</param>
    /// <returns>An LtiLaunch object representing the constructed LTI launch request.</returns>
    LtiLaunch GetLtiLaunch(Uri issuer, Tool tool, ResourceLink resourceLink, UserId? userId = null, UserId? actualUserId = null, bool isAnonymous = false, LaunchPresentationOverride? launchPresentationOverride = null);
}

/// <summary>
/// Handles LTI 1.3 Resource Link Request messages, providing methods to construct and process LTI launches and message payloads for resource link interactions.
/// </summary>
/// <remarks>This handler coordinates the retrieval and assembly of LTI launch data, including user, context, and resource link information, and supports extensibility through message extensions.
/// It is typically used as part of an LTI 1.3 tool integration to process launch requests and generate appropriate LTI message payloads.</remarks>
/// <param name="coreDataService">The service used to access core LTI 1.3 tool and platform data.</param>
/// <param name="dataService">The service used to retrieve resource link, deployment, context, user, and related LTI message data.</param>
/// <param name="tokenConfigService">The service that provides token configuration information for LTI 1.3 tools.</param>
/// <param name="platformService">The service used to access platform-specific information for LTI 1.3 integrations.</param>
/// <param name="extensions">A collection of extensions that can augment or customize the LTI Resource Link Request message.</param>
/// <param name="logger">The logger used to record diagnostic and operational information for this handler.</param>
internal class Lti13ResourceLinkRequestMessageHandler(
    ILti13CoreDataService coreDataService,
    ILti13ResourceLinkMessageDataService dataService,
    ILti13TokenConfigService tokenConfigService,
    ILti13PlatformService platformService,
    IEnumerable<ILti13ResourceLinkMessageExtension> extensions,
    ILogger<Lti13ResourceLinkRequestMessageHandler> logger)
    : ILti13ResourceLinkRequestMessageHandler
{
    private static readonly string MessageType = "LtiResourceLinkRequest";

    /// <inheritdoc/>
    public async Task<LtiLaunch?> GetLtiLaunchAsync(ResourceLinkId resourceLinkId, UserId? userId, UserId? actualUserId, bool isAnonymous, LaunchPresentationOverride? launchPresentationOverride = null, CancellationToken cancellationToken = default)
    {
        var resourceLink = await dataService.GetResourceLinkAsync(resourceLinkId, cancellationToken);

        var deployment = resourceLink != null
            ? await dataService.GetDeploymentAsync(resourceLink.DeploymentId, cancellationToken)
            : null;

        var tool = deployment != null
            ? await coreDataService.GetToolAsync(deployment.ClientId, cancellationToken)
            : null;

        if (resourceLink == null
            || tool == null)
        {
            return null;
        }

        var tokenConfig = await tokenConfigService.GetTokenConfigAsync(tool.ClientId, cancellationToken);

        return GetLtiLaunch(tokenConfig.Issuer, tool, resourceLink, userId, actualUserId, isAnonymous, launchPresentationOverride);
    }

    /// <inheritdoc/>
    public LtiLaunch GetLtiLaunch(Uri issuer, Tool tool, ResourceLink resourceLink, UserId? userId = null, UserId? actualUserId = null, bool isAnonymous = false, LaunchPresentationOverride? launchPresentationOverride = null)
    {
        var loginHint = new LoginHint(userId, actualUserId, isAnonymous);
        var ltiMessageHint = new LtiMessageHint(resourceLink.Id, launchPresentationOverride);

        return new LtiLaunch(tool, issuer, resourceLink.Url ?? tool.LaunchUrl, resourceLink.DeploymentId, loginHint.ToString(), ltiMessageHint.ToString());
    }

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

    /// <inheritdoc/>
    public async Task<LtiMessageResult> HandleLtiMessageAsync(
        string loginHint,
        string? ltiMessageHint,
        Tool tool,
        string nonce,
        CancellationToken cancellationToken = default)
    {
        if (ltiMessageHint == null
            || !LtiMessageHint.TryParse(ltiMessageHint, out var ltiMessageHintRecord))
        {
            return LtiMessageResult.None();
        }

        if (!LoginHint.TryParse(loginHint, out var loginHintRecord))
        {
            return LtiMessageResult.None();
        }

        var resourceLink = await dataService.GetResourceLinkAsync(ltiMessageHintRecord.ResourceLinkId, cancellationToken);
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
        if (loginHintRecord.UserId != null)
        {
            user = await dataService.GetUserAsync(loginHintRecord.UserId.Value, cancellationToken);
            if (user == null)
            {
                return LtiMessageResult.Error("");
            }
        }

        User? actualUser = null;
        if (loginHintRecord.ActualUserId != null)
        {
            actualUser = await dataService.GetUserAsync(loginHintRecord.ActualUserId.Value, cancellationToken);
            if (actualUser == null)
            {
                return LtiMessageResult.Error("");
            }
        }

        var tokenConfig = await tokenConfigService.GetTokenConfigAsync(tool.ClientId, cancellationToken);
        var platform = await platformService.GetPlatformAsync(tool.ClientId, cancellationToken);
        var customPermissions = await dataService.GetCustomPermissionsAsync(deployment.Id, context.Id, loginHintRecord.UserId, loginHintRecord.ActualUserId, cancellationToken);

        var lineItems = await dataService.GetLineItemsAsync(resourceLink.Id, pageIndex: 0, limit: 1, cancellationToken: cancellationToken);
        var lineItem = lineItems.TotalItems == 1 ? lineItems.Items.First() : null;
        var grade = lineItem != null && user != null
            ? await dataService.GetGradeAsync(lineItem.Id, user.Id, cancellationToken)
            : null;

        var userPermissions = user != null
            ? await dataService.GetUserPermissionsAsync(deployment.Id, context.Id, user.Id, cancellationToken)
            : null;
        var userMembership = user != null
            ? await dataService.GetMembershipAsync(context.Id, user.Id, cancellationToken)
            : null;
        var attempt = user != null
            ? await dataService.GetAttemptAsync(ltiMessageHintRecord.ResourceLinkId, user.Id, cancellationToken)
            : null;
        var actualUserMembership = actualUser != null
            ? await dataService.GetMembershipAsync(context.Id, actualUser.Id, cancellationToken)
            : null;

        var ltiMessage = new LtiResourceLinkRequestMessage()
            .WithLtiMessageClaims(
                MessageType,
                nonce,
                tool.ClientId,
                tokenConfig)
            .WithLtiVersionClaims()
            .WithDeploymentIdClaims(deployment.Id)
            .WithTargetLinkUriClaims(resourceLink.Url ?? tool.LaunchUrl)
            .WithResourceLinkClaims(resourceLink)
            .WithContextClaims(context)
            .WithCustomClaims(
                customPermissions,
                platform,
                tool,
                deployment,
                context,
                resourceLink,
                !loginHintRecord.IsAnonymous ? userMembership : null,
                !loginHintRecord.IsAnonymous ? user : null,
                !loginHintRecord.IsAnonymous ? actualUserMembership : null,
                !loginHintRecord.IsAnonymous ? actualUser : null,
                lineItem,
                attempt,
                grade);

        if (platform != null)
        {
            ltiMessage = ltiMessage
                .WithPlatformInstanceClaims(platform);
        }

        if (ltiMessageHintRecord.LaunchPresentationOverride != null)
        {
            ltiMessage = ltiMessage
               .WithLaunchPresentationClaims(ltiMessageHintRecord.LaunchPresentationOverride);
        }

        if (userMembership != null)
        {
            ltiMessage = ltiMessage
                .WithRolesClaims(userMembership, logger);

            if (!loginHintRecord.IsAnonymous)
            {
                ltiMessage = ltiMessage
                    .WithRoleScopeMentorClaims(userMembership);
            }
        }

        if (!loginHintRecord.IsAnonymous
            && userPermissions != null
            && user != null)
        {
            ltiMessage = ltiMessage
                .WithUserIdentityClaims(userPermissions, user);
        }

        if (extensions.Any())
        {
            var extensionResults = await Task.WhenAll(extensions.Select(e => e.GetMessageExtensionAsync(tool, resourceLink, user, cancellationToken)));
            var extendedMessage = ltiMessage.Extend(extensionResults);
            return LtiMessageResult.Success(extendedMessage);
        }
        else
        {
            return LtiMessageResult.Success(ltiMessage);
        }
    }

    private record LtiMessageHint(ResourceLinkId ResourceLinkId, LaunchPresentationOverride? LaunchPresentationOverride)
    {
        public override string ToString()
        {
            var launchPresentationOverrideString = LaunchPresentationOverride != null
                ? JsonSerializer.Serialize(LaunchPresentationOverride)
                : string.Empty;

            return $"{MessageType}|{ResourceLinkId}|{launchPresentationOverrideString}";
        }

        public static bool TryParse(string ltiMessageHintString, out LtiMessageHint ltiMessageHint)
        {
            if (ltiMessageHintString.Split('|', 3, StringSplitOptions.TrimEntries) is not [var messageTypeString, var resourceLinkIdString, var launchPresentationOverrideString]
                || messageTypeString != MessageType
                || !ResourceLinkId.TryParse(resourceLinkIdString, null, out var resourceLinkId)
                || !TryDeserialize<LaunchPresentationOverride>(launchPresentationOverrideString, out var launchPresentationOverride))
            {
                ltiMessageHint = new LtiMessageHint(ResourceLinkId.Empty, null);
                return false;
            }

            ltiMessageHint = new LtiMessageHint(resourceLinkId, launchPresentationOverride);
            return true;
        }
    }

    private record LoginHint(UserId? UserId, UserId? ActualUserId, bool IsAnonymous)
    {
        public override string ToString()
        {
            return $"{UserId}|{ActualUserId}|{IsAnonymous}";
        }

        public static bool TryParse(string loginHintString, out LoginHint loginHint)
        {
            if (loginHintString.Split('|', 3, StringSplitOptions.TrimEntries) is not [var userIdString, var actualUserIdString, var isAnonymousString]
                || !bool.TryParse(isAnonymousString, out var isAnonymous))
            {
                loginHint = new LoginHint(null, null, true);
                return false;
            }

            loginHint = new LoginHint(new UserId(userIdString), new UserId(actualUserIdString), isAnonymous);
            return true;
        }
    }
}

/// <summary>
/// Defines a contract for retrieving a message extension object associated with a specific tool, resource link, and user in an LTI (Learning Tools Interoperability) context.
/// </summary>
public interface ILti13ResourceLinkMessageExtension
{
    /// <summary>
    /// Asynchronously retrieves a message extension object for the specified tool, resource link, and user.
    /// </summary>
    /// <param name="tool">The tool for which to retrieve the message extension. Cannot be null.</param>
    /// <param name="resourceLink">The resource link associated with the message extension. Cannot be null.</param>
    /// <param name="user">The user for whom the message extension is being retrieved. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the message extension object, or null if no extension is available.</returns>
    Task<object> GetMessageExtensionAsync(Tool tool, ResourceLink resourceLink, User? user, CancellationToken cancellationToken = default);
}