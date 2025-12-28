using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Scopes;
using NP.Lti13Platform.Core.Services;

namespace NP.Lti13Platform.Core;

public interface ILtiMessageHandler
{
    Task<LtiMessageResult> HandleLtiMessageAsync(string loginHint, string? ltiMessageHint, Tool tool, CancellationToken cancellationToken = default);
}

public abstract class LtiMessageHandler<T>
    : ILtiMessageHandler where T : ILtiMessage
{
    async Task<LtiMessageResult> ILtiMessageHandler.HandleLtiMessageAsync(string loginHint, string? ltiMessageHint, Tool tool, CancellationToken cancellationToken)
    {
        return await HandleLtiMessageAsync(loginHint, ltiMessageHint, tool, cancellationToken);
    }

    public abstract Task<LtiMessageResult<T>> HandleLtiMessageAsync(string loginHint, string? ltiMessageHint, Tool tool, CancellationToken cancellationToken = default);
}

internal class LtiResourceLinkMessageHandler(
    ILti13CoreDataService dataService,
    ILti13TokenConfigService tokenConfigService)
    : LtiMessageHandler<LtiResourceLinkRequestMessage>
{
    public override async Task<LtiMessageResult<LtiResourceLinkRequestMessage>> HandleLtiMessageAsync(
        string loginHint,
        string? ltiMessageHint,
        Tool tool,
        CancellationToken cancellationToken = default)
    {
        if (loginHint.Split('|', 2, StringSplitOptions.TrimEntries) is not [var userIdString, var actualUserIdString])
        {
            return LtiMessageResult<LtiResourceLinkRequestMessage>.None();
        }

        if (ltiMessageHint?.Split('|', 2, StringSplitOptions.TrimEntries) is not [var messageTypeString, var resourceLinkIdString]
            || messageTypeString != Lti13MessageType.LtiResourceLinkRequest
            || ResourceLinkId.TryParse(resourceLinkIdString, null, out var resourceLinkId))
        {
            return LtiMessageResult<LtiResourceLinkRequestMessage>.None();
        }

        var resourceLink = await dataService.GetResourceLinkAsync(resourceLinkId, cancellationToken);
        if (resourceLink == null)
        {
            return LtiMessageResult<LtiResourceLinkRequestMessage>.Fail("");
        }

        var context = await dataService.GetContextAsync(resourceLink.ContextId, cancellationToken);
        if (context == null)
        {
            return LtiMessageResult<LtiResourceLinkRequestMessage>.Fail("");
        }

        var deployment = await dataService.GetDeploymentAsync(resourceLink.DeploymentId, cancellationToken);
        if (deployment == null || deployment.ClientId != tool.ClientId)
        {
            return LtiMessageResult<LtiResourceLinkRequestMessage>.Fail("");
        }

        User? user = null;
        User? actualUser = null;
        if (UserId.TryParse(userIdString, null, out var userId)
            && userId != UserId.Empty)
        {
            user = await dataService.GetUserAsync(userId, cancellationToken);
            if (user == null)
            {
                return LtiMessageResult<LtiResourceLinkRequestMessage>.Fail("");
            }

            if (UserId.TryParse(actualUserIdString, null, out var actualUserId)
                && actualUserId != UserId.Empty)
            {
                actualUser = await dataService.GetUserAsync(actualUserId, cancellationToken);
                if (actualUser == null)
                {
                    return LtiMessageResult<LtiResourceLinkRequestMessage>.Fail("");
                }
            }
        }

        var tokenConfig = await tokenConfigService.GetTokenConfigAsync(tool.ClientId, cancellationToken);

        var ltiMessage = new LtiResourceLinkRequestMessage
        {
            Audience = "",
            Issuer = "",
            ResourceLink = "",
            Nonce = "",
            TargetLinkUri = ""
        };

        ltiMessage.Audience = tool.ClientId.ToString();
        ltiMessage.IssuedDate = DateTime.UtcNow;
        ltiMessage.Issuer = tokenConfig.Issuer.OriginalString;
        ltiMessage.Nonce = request.Nonce!;
        ltiMessage.ExpirationDate = DateTime.UtcNow.AddSeconds(tokenConfig.MessageTokenExpirationSeconds);

        if (user != null)
        {
            var userPermissions = await dataService.GetUserPermissionsAsync(deployment.Id, contextId, user.Id, cancellationToken);

            ltiMessage.Subject = user.Id.ToString();

            ltiMessage.Address = user.Address == null || !userPermissions.Address ? null : new AddressClaim
            {
                Country = userPermissions.AddressCountry ? user.Address.Country : null,
                Formatted = userPermissions.AddressFormatted ? user.Address.Formatted : null,
                Locality = userPermissions.AddressLocality ? user.Address.Locality : null,
                PostalCode = userPermissions.AddressPostalCode ? user.Address.PostalCode : null,
                Region = userPermissions.AddressRegion ? user.Address.Region : null,
                StreetAddress = userPermissions.AddressStreetAddress ? user.Address.StreetAddress : null
            };

            ltiMessage.Birthdate = userPermissions.Birthdate ? user.Birthdate : null;
            ltiMessage.Email = userPermissions.Email ? user.Email : null;
            ltiMessage.EmailVerified = userPermissions.EmailVerified ? user.EmailVerified : null;
            ltiMessage.FamilyName = userPermissions.FamilyName ? user.FamilyName : null;
            ltiMessage.Gender = userPermissions.Gender ? user.Gender : null;
            ltiMessage.GivenName = userPermissions.GivenName ? user.GivenName : null;
            ltiMessage.Locale = userPermissions.Locale ? user.Locale : null;
            ltiMessage.MiddleName = userPermissions.MiddleName ? user.MiddleName : null;
            ltiMessage.Name = userPermissions.Name ? user.Name : null;
            ltiMessage.Nickname = userPermissions.Nickname ? user.Nickname : null;
            ltiMessage.PhoneNumber = userPermissions.PhoneNumber ? user.PhoneNumber : null;
            ltiMessage.PhoneNumberVerified = userPermissions.PhoneNumberVerified ? user.PhoneNumberVerified : null;
            ltiMessage.Picture = userPermissions.Picture ? user.Picture?.OriginalString : null;
            ltiMessage.PreferredUsername = userPermissions.PreferredUsername ? user.PreferredUsername : null;
            ltiMessage.Profile = userPermissions.Profile ? user.Profile?.OriginalString : null;
            ltiMessage.UpdatedAt = userPermissions.UpdatedAt ? user.UpdatedAt : null;
            ltiMessage.Website = userPermissions.Website ? user.Website?.OriginalString : null;
            ltiMessage.TimeZone = userPermissions.TimeZone ? user.TimeZone : null;
        }

        return LtiMessageResult<LtiResourceLinkRequestMessage>.Success(ltiMessage);
    }
}

public record LtiResourceLinkRequestMessage
    : ILtiMessage,
    IContextMessage,
    ICustomMessage,
    ILaunchPresentationMessage,
    IPlatformMessage,
    IResourceLinkMessage,
    IRolesMessage
{
    /// <inheritdoc />
    public required string Issuer { get; set; }
    /// <inheritdoc />
    public required string Audience { get; set; }
    /// <inheritdoc />
    public DateTime ExpirationDate { get; set; } = DateTime.UtcNow;
    /// <inheritdoc />
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
    /// <inheritdoc />
    public required string Nonce { get; set; }
    /// <inheritdoc />
    public string MessageType => Lti13MessageType.LtiResourceLinkRequest;
    /// <inheritdoc />
    public string? Subject { get; set; }
    /// <inheritdoc />
    public string? Name { get; set; }
    /// <inheritdoc />
    public string? GivenName { get; set; }
    /// <inheritdoc />
    public string? FamilyName { get; set; }
    /// <inheritdoc />
    public string? MiddleName { get; set; }
    /// <inheritdoc />
    public string? Nickname { get; set; }
    /// <inheritdoc />
    public string? PreferredUsername { get; set; }
    /// <inheritdoc />
    public string? Profile { get; set; }
    /// <inheritdoc />
    public string? Picture { get; set; }
    /// <inheritdoc />
    public string? Website { get; set; }
    /// <inheritdoc />
    public string? Email { get; set; }
    /// <inheritdoc />
    public bool? EmailVerified { get; set; }
    /// <inheritdoc />
    public string? Gender { get; set; }
    /// <inheritdoc />
    public DateOnly? Birthdate { get; set; }
    /// <inheritdoc />
    public string? TimeZone { get; set; }
    /// <inheritdoc />
    public string? Locale { get; set; }
    /// <inheritdoc />
    public string? PhoneNumber { get; set; }
    /// <inheritdoc />
    public bool? PhoneNumberVerified { get; set; }
    /// <inheritdoc />
    public AddressClaim? Address { get; set; }
    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }
    /// <inheritdoc />
    public IContextMessage.MessageContext? Context { get; set; }
    /// <inheritdoc />
    public IDictionary<string, string>? Custom { get; set; }
    /// <inheritdoc />
    public ILaunchPresentationMessage.LaunchPresentationDefinition? LaunchPresentation { get; set; }
    /// <inheritdoc />
    public IPlatformMessage.ToolPlatform? Platform { get; set; }
    /// <inheritdoc />
    public string LtiVersion => "1.3.0";
    /// <inheritdoc />
    public DeploymentId DeploymentId { get; set; }
    /// <inheritdoc />
    public required string TargetLinkUri { get; set; }
    /// <inheritdoc />
    public required IResourceLinkMessage.ResourceLinkMessage ResourceLink { get; set; }
    /// <inheritdoc />
    public IEnumerable<string> Roles { get; set; } = [];
    /// <inheritdoc />
    public IEnumerable<UserId>? RoleScopeMentor { get; set; }
}