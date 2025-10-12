using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.DeepLinking.Configs;

namespace NP.Lti13Platform.DeepLinking.Services;

/// <summary>
/// Defines a service for retrieving configuration settings for deep linking in an LTI 1.3 platform.
/// </summary>
public interface ILti13DeepLinkingConfigService
{
    /// <summary>
    /// Gets the configuration for deep linking.
    /// </summary>
    /// <param name="clientId">The tool identifier for which to retrieve the configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deep linking configuration.</returns>
    Task<DeepLinkingConfig> GetConfigAsync(ClientId clientId, CancellationToken cancellationToken = default);
}

internal class DefaultDeepLinkingConfigService(IOptionsMonitor<DeepLinkingConfig> config, IHttpContextAccessor httpContextAccessor) : ILti13DeepLinkingConfigService
{
    public async Task<DeepLinkingConfig> GetConfigAsync(ClientId clientId, CancellationToken cancellationToken = default)
    {
        var deepLinkingConfig = config.CurrentValue;
        if (deepLinkingConfig.ServiceAddress == DeepLinkingConfig.DefaultUri)
        {
            deepLinkingConfig = deepLinkingConfig with { ServiceAddress = new UriBuilder(httpContextAccessor.HttpContext?.Request.Scheme, httpContextAccessor.HttpContext?.Request.Host.Value).Uri };
        }

        return await Task.FromResult(deepLinkingConfig);
    }
}
