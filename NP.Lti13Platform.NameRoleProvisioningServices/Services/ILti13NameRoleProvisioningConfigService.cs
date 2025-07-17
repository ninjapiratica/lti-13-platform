using NP.Lti13Platform.NameRoleProvisioningServices.Configs;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Services;

/// <summary>
/// Defines a service for retrieving configuration settings for the name and role provisioning service in an LTI 1.3 platform.
/// </summary>
public interface ILti13NameRoleProvisioningConfigService
{
    /// <summary>
    /// Gets the configuration for name and role provisioning services.
    /// </summary>
    /// <param name="clientId">The client identifier for which to retrieve the configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the services configuration.</returns>
    Task<ServicesConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default);
}
