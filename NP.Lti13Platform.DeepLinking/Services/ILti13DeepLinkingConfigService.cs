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
    /// <param name="toolId">The tool identifier for which to retrieve the configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deep linking configuration.</returns>
    Task<DeepLinkingConfig> GetConfigAsync(string toolId, CancellationToken cancellationToken = default);
}
