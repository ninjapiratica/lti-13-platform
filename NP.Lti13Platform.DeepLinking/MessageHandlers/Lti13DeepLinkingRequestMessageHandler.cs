using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Extensions;
using NP.Lti13Platform.Core.MessageClaims;
using NP.Lti13Platform.Core.MessageHandlers;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using NP.Lti13Platform.DeepLinking.MessageClaims;
using NP.Lti13Platform.DeepLinking.Services;
using System.Text.Json;

namespace NP.Lti13Platform.DeepLinking.MessageHandlers;

/// <summary>
/// Defines methods for handling LTI deep linking request messages, including retrieving and creating LTI launch objects with support for context, user, and presentation overrides.
/// </summary>
public interface ILti13DeepLinkingRequestMessageHandler : ILtiMessageHandler
{
    /// <summary>
    /// Asynchronously retrieves an LTI launch for the specified deployment, context, and user parameters, optionally applying presentation and deep linking overrides.
    /// </summary>
    /// <param name="deploymentId">The unique identifier of the LTI deployment for which to retrieve the launch. Cannot be null.</param>
    /// <param name="contextId">The identifier of the LTI context (such as a course or group) associated with the launch, or null to indicate a contextless launch.</param>
    /// <param name="userId">The identifier of the user for whom the launch is being performed, or null if the launch is not associated with a specific user.</param>
    /// <param name="actualUserId">The identifier of the actual user performing the launch, if different from <paramref name="userId"/> (for example, in cases of acting on behalf of another user); otherwise, null.</param>
    /// <param name="isAnonymous">A value indicating whether the launch should be performed in anonymous mode.
    /// Set to <see langword="true"/> to omit user-identifying information from the launch; otherwise, <see langword="false"/>.</param>
    /// <param name="deepLinkingUrl">The deep linking return URL to use for the launch, or null if deep linking is not required.</param>
    /// <param name="launchPresentationOverride">An optional object specifying overrides for the launch presentation parameters, such as display target or window preferences.
    /// If null, default presentation settings are used.</param>
    /// <param name="deepLinkingSettingsOverride">An optional object specifying overrides for deep linking settings. If null, default deep linking settings are used.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="LtiLaunch"/> if a matching launch is found; otherwise, null.</returns>
    Task<LtiLaunch?> GetLtiLaunchAsync(
        DeploymentId deploymentId,
        ContextId? contextId = null,
        UserId? userId = null,
        UserId? actualUserId = null,
        bool isAnonymous = false,
        Uri? deepLinkingUrl = null,
        LaunchPresentationOverride? launchPresentationOverride = null,
        DeepLinkingSettingsOverride? deepLinkingSettingsOverride = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an LTI launch object for the specified issuer, tool, and deployment, with optional overrides for context, user, and launch settings.
    /// </summary>
    /// <param name="issuer">The issuer URI representing the platform initiating the LTI launch. Cannot be null.</param>
    /// <param name="tool">The tool configuration to use for the launch. Cannot be null.</param>
    /// <param name="deploymentId">The deployment identifier associated with the tool registration. Cannot be null.</param>
    /// <param name="contextId">The context identifier for the launch, such as a course or group. If null, the launch will not be associated with a specific context.</param>
    /// <param name="userId">The identifier of the user for whom the launch is being performed. If null, the launch may be performed without a user context.</param>
    /// <param name="actualUserId">The identifier of the actual user performing the launch, if different from <paramref name="userId"/> (e.g., for acting on behalf of another user). If null, no distinction is made.</param>
    /// <param name="isAnonymous">true if the launch should be performed without user-identifying information; otherwise, false.</param>
    /// <param name="deepLinkingUrl">The deep linking return URL to use for the launch, if applicable. If null, deep linking is not performed.</param>
    /// <param name="launchPresentationOverride">An optional override for launch presentation parameters, such as display or window settings. If null, default presentation settings are used.</param>
    /// <param name="deepLinkingSettingsOverride">An optional override for deep linking settings. If null, default deep linking settings are used.</param>
    /// <returns>An LtiLaunch object representing the configured LTI launch. The returned object contains all parameters and overrides applied.</returns>
    LtiLaunch GetLtiLaunch(
        Uri issuer,
        Tool tool,
        DeploymentId deploymentId,
        ContextId? contextId = null,
        UserId? userId = null,
        UserId? actualUserId = null,
        bool isAnonymous = false,
        Uri? deepLinkingUrl = null,
        LaunchPresentationOverride? launchPresentationOverride = null,
        DeepLinkingSettingsOverride? deepLinkingSettingsOverride = null);
}

internal class Lti13DeepLinkingRequestMessageHandler(
    ILti13CoreDataService coreDataService,
    ILti13DeepLinkingRequestDataService dataService,
    ILti13DeepLinkingConfigService deepLinkingConfigService,
    ILti13TokenConfigService tokenConfigService,
    ILti13PlatformService platformService,
    IEnumerable<ILti13DeepLinkingMessageExtension> extensions,
    ILogger<Lti13DeepLinkingRequestMessageHandler> logger,
    LinkGenerator linkGenerator)
    : ILti13DeepLinkingRequestMessageHandler
{
    public static readonly string MessageType = "LtiDeepLinkingRequest";

    /// <inheritdoc/>
    public async Task<LtiLaunch?> GetLtiLaunchAsync(
        DeploymentId deploymentId,
        ContextId? contextId,
        UserId? userId,
        UserId? actualUserId,
        bool isAnonymous,
        Uri? deepLinkingUrl,
        LaunchPresentationOverride? launchPresentationOverride = null,
        DeepLinkingSettingsOverride? deepLinkingSettingsOverride = null,
        CancellationToken cancellationToken = default)
    {
        var deployment = await dataService.GetDeploymentAsync(deploymentId, cancellationToken);

        var tool = deployment != null
            ? await coreDataService.GetToolAsync(deployment.ClientId, cancellationToken)
            : null;

        if (deployment == null
            || tool == null)
        {
            return null;
        }

        var tokenConfig = await tokenConfigService.GetTokenConfigAsync(tool.ClientId, cancellationToken);

        return GetLtiLaunch(tokenConfig.Issuer, tool, deploymentId, contextId, userId, actualUserId, isAnonymous, deepLinkingUrl, launchPresentationOverride, deepLinkingSettingsOverride);
    }

    /// <inheritdoc/>
    public LtiLaunch GetLtiLaunch(
        Uri issuer,
        Tool tool,
        DeploymentId deploymentId,
        ContextId? contextId = null,
        UserId? userId = null,
        UserId? actualUserId = null,
        bool isAnonymous = false,
        Uri? deepLinkingUrl = null,
        LaunchPresentationOverride? launchPresentationOverride = null,
        DeepLinkingSettingsOverride? deepLinkingSettingsOverride = null)
    {
        var loginHint = new LoginHint(userId, actualUserId, isAnonymous);
        var ltiMessageHint = new LtiMessageHint(deploymentId, contextId, launchPresentationOverride, deepLinkingSettingsOverride);

        return new LtiLaunch(tool, issuer, deepLinkingUrl ?? tool.LaunchUrl, deploymentId, loginHint.ToString(), ltiMessageHint.ToString());
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

        var deployment = await dataService.GetDeploymentAsync(ltiMessageHintRecord.DeploymentId, cancellationToken);
        if (deployment == null || deployment.ClientId != tool.ClientId)
        {
            return LtiMessageResult.Error("");
        }

        Context? context = null;
        if (ltiMessageHintRecord.ContextId != null
            && ltiMessageHintRecord.ContextId != ContextId.Empty)
        {
            context = await dataService.GetContextAsync(ltiMessageHintRecord.ContextId.Value, cancellationToken);

            if (context == null)
            {
                return LtiMessageResult.Error("");
            }
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

        var deepLinkingConfig = await deepLinkingConfigService.GetConfigAsync(tool.ClientId, cancellationToken);
        var tokenConfig = await tokenConfigService.GetTokenConfigAsync(tool.ClientId, cancellationToken);
        var platform = await platformService.GetPlatformAsync(tool.ClientId, cancellationToken);
        var customPermissions = await dataService.GetCustomPermissionsAsync(deployment.Id, context?.Id, loginHintRecord.UserId, loginHintRecord.ActualUserId, cancellationToken);

        var userPermissions =
            user != null
            && context != null
                ? await dataService.GetUserPermissionsAsync(deployment.Id, context.Id, user.Id, cancellationToken)
                : null;
        var userMembership =
            user != null
            && context != null
                ? await dataService.GetMembershipAsync(context.Id, user.Id, cancellationToken)
                : null;
        var actualUserMembership = actualUser != null
            && context != null
                ? await dataService.GetMembershipAsync(context.Id, actualUser.Id, cancellationToken)
                : null;

        var ltiMessage = new LtiDeepLinkingRequestMessage()
            .WithDeepLinkingSettingsClaims(deepLinkingConfig, linkGenerator, ltiMessageHintRecord.ContextId, ltiMessageHintRecord.DeepLinkingSettingsOverride)
            .WithLtiMessageClaims(
                MessageType,
                nonce,
                tool.ClientId,
                tokenConfig)
            .WithLtiVersionClaims()
            .WithDeploymentIdClaims(deployment.Id)
            .WithCustomClaims(
                customPermissions,
                platform: platform,
                tool: tool,
                deployment: deployment,
                context: context,
                userMembership: !loginHintRecord.IsAnonymous ? userMembership : null,
                user: !loginHintRecord.IsAnonymous ? user : null,
                actualUserMembership: !loginHintRecord.IsAnonymous ? actualUserMembership : null,
                actualUser: !loginHintRecord.IsAnonymous ? actualUser : null);

        if (context != null)
        {
            ltiMessage = ltiMessage
                .WithContextClaims(context);
        }

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
            var extensionResults = await Task.WhenAll(extensions.Select(e => e.GetMessageExtensionAsync(tool, deployment, context, user, cancellationToken)));
            var extendedMessage = ltiMessage.Extend(extensionResults);
            return LtiMessageResult.Success(extendedMessage);
        }
        else
        {
            return LtiMessageResult.Success(ltiMessage);
        }
    }

    private record LtiMessageHint(DeploymentId DeploymentId, ContextId? ContextId, LaunchPresentationOverride? LaunchPresentationOverride, DeepLinkingSettingsOverride? DeepLinkingSettingsOverride)
    {
        public override string ToString()
        {
            var launchPresentationOverrideString = LaunchPresentationOverride != null
                ? JsonSerializer.Serialize(LaunchPresentationOverride)
                : string.Empty;

            var deepLinkingSettingsOverrideString = DeepLinkingSettingsOverride != null
                ? JsonSerializer.Serialize(DeepLinkingSettingsOverride)
                : string.Empty;

            return $"{MessageType}|{DeploymentId}|{ContextId}|{launchPresentationOverrideString}|{deepLinkingSettingsOverrideString}";
        }

        public static bool TryParse(string ltiMessageHintString, out LtiMessageHint ltiMessageHint)
        {
            if (ltiMessageHintString.Split('|', 5, StringSplitOptions.TrimEntries) is not [var messageTypeString, var deploymentIdString, var contextIdString, var launchPresentationOverrideString, var deepLinkingSettingsOverrideString]
                || messageTypeString != MessageType
                || !DeploymentId.TryParse(deploymentIdString, null, out var deploymentId)
                || !TryDeserialize<LaunchPresentationOverride>(launchPresentationOverrideString, out var launchPresentationOverride)
                || !TryDeserialize<DeepLinkingSettingsOverride>(deepLinkingSettingsOverrideString, out var deepLinkingSettingsOverride))
            {
                ltiMessageHint = new LtiMessageHint(DeploymentId.Empty, null, null, null);
                return false;
            }

            // Parse ContextId is optional
            Core.Models.ContextId.TryParse(contextIdString, null, out var contextId);

            ltiMessageHint = new LtiMessageHint(deploymentId, contextId, launchPresentationOverride, deepLinkingSettingsOverride);
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