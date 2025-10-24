using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.Services;

/// <summary>
/// Defines methods for retrieving security configurations for LTI 1.3 tools.
/// </summary>
/// <remarks>This service is used to manage and retrieve security settings associated with LTI 1.3 tools. It
/// provides functionality to fetch security configurations based on the tool's client identifier.</remarks>
public interface ILti13ToolSecurityService
{
    /// <summary>
    /// Retrieves the security configuration for an LTI 1.3 tool based on the specified client identifier.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client for which the tool security configuration is requested.</param>
    /// <param name="baseUrl">The base url of the authentication, token and jwks endpoints.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Lti13ToolSecurity"/>
    /// object representing the security configuration for the specified client.</returns>
    Task<Lti13ToolSecurity> GetToolSecurityAsync(ClientId clientId, Uri baseUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the security configuration for an LTI 1.3 tool based on the specified client identifier.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client for which the tool security configuration is requested.</param>
    /// <param name="authenticationBaseUrl">The base url of the authentication endpoints.</param>
    /// <param name="tokenBaseUrl">The base url of the token endpoint.</param>
    /// <param name="jwksBaseUrl">The base url of the jwks endpoint.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Lti13ToolSecurity"/>
    /// object representing the security configuration for the specified client.</returns>
    Task<Lti13ToolSecurity> GetToolSecurityAsync(ClientId clientId, Uri authenticationBaseUrl, Uri tokenBaseUrl, Uri jwksBaseUrl, CancellationToken cancellationToken = default);
}

internal class DefaultLti13ToolSecurityService(ILti13TokenConfigService tokenConfigService, LinkGenerator linkGenerator) : ILti13ToolSecurityService
{
    public async Task<Lti13ToolSecurity> GetToolSecurityAsync(ClientId clientId, Uri baseUrl, CancellationToken cancellationToken)
        => await GetToolSecurityAsync(clientId, baseUrl, baseUrl, baseUrl, cancellationToken);

    public async Task<Lti13ToolSecurity> GetToolSecurityAsync(ClientId clientId, Uri authenticationBaseUrl, Uri tokenBaseUrl, Uri jwksBaseUrl, CancellationToken cancellationToken = default)
    {
        var tokenConfig = await tokenConfigService.GetTokenConfigAsync(clientId, cancellationToken);

        var authGetUrl = linkGenerator.GetUriByName(RouteNames.AUTHENTICATION_GET, (object?)null, authenticationBaseUrl.Scheme, HostString.FromUriComponent(authenticationBaseUrl))!;
        var authPostUrl = linkGenerator.GetUriByName(RouteNames.AUTHENTICATION_POST, (object?)null, authenticationBaseUrl.Scheme, HostString.FromUriComponent(authenticationBaseUrl))!;
        var tokenUrl = linkGenerator.GetUriByName(RouteNames.TOKEN, (object?)null, tokenBaseUrl.Scheme, HostString.FromUriComponent(tokenBaseUrl))!;
        var jwksUrl = linkGenerator.GetUriByName(RouteNames.JWKS, new { clientId }, jwksBaseUrl.Scheme, HostString.FromUriComponent(jwksBaseUrl))!;

        return new Lti13ToolSecurity(
            clientId,
            tokenConfig.Issuer,
            new Uri(authGetUrl),
            new Uri(authPostUrl),
            new Uri(tokenUrl),
            new Uri(jwksUrl)
        );
    }
}

/// <summary>
/// Represents the security configuration for an LTI 1.3 tool, including client identification and endpoint URLs for
/// authentication and token exchange.
/// </summary>
/// <param name="ClientId">The unique identifier for the client application. This value is required and must be valid.</param>
/// <param name="Issuer">The issuer URI that identifies the authorization server. This value is required and must be a valid URI.</param>
/// <param name="GetAuthenticationUrl">The URL used to initiate the authentication process. This value is required and must be a valid URI.</param>
/// <param name="PostAuthenticationUrl">The URL used to initiate the authentication process. This value is required and must be a valid URI.</param>
/// <param name="TokenUrl">The URL used to exchange authorization codes for access tokens. This value is required and must be a valid URI.</param>
/// <param name="JwksUrl">The URL of the JSON Web Key Set (JWKS) endpoint used to retrieve public keys for validating tokens. This value is
/// required and must be a valid URI.</param>
public record Lti13ToolSecurity(ClientId ClientId, Uri Issuer, Uri GetAuthenticationUrl, Uri PostAuthenticationUrl, Uri TokenUrl, Uri JwksUrl);