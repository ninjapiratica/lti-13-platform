using NP.Lti13Platform.AssignmentGradeServices.Configs;
using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.DeepLinking.Configs;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;

namespace NP.Lti13Platform;

/// <summary>
/// Represents the configuration for LTI 1.3 platform endpoints.
/// </summary>
/// <remarks>
/// This class provides access to various endpoint configurations used in LTI 1.3 integrations, including core endpoints, deep linking, name and role provisioning services, and assignment and grade services.
/// </remarks>
public class Lti13PlatformEndpointsConfig
{
    /// <summary>
    /// Gets or sets the configuration for core LTI 1.3 platform endpoints.
    /// </summary>
    public Lti13PlatformCoreEndpointsConfig Core { get; set; } = new();

    /// <summary>
    /// Gets or sets the configuration for LTI 1.3 deep linking endpoints.
    /// </summary>
    public DeepLinkingEndpointsConfig DeepLinking { get; set; } = new();

    /// <summary>
    /// Gets or sets the configuration for LTI 1.3 name and role provisioning services endpoints.
    /// </summary>
    public EndpointsConfig NameRoleProvisioningServices { get; set; } = new();

    /// <summary>
    /// Gets or sets the configuration for LTI 1.3 assignment and grade services endpoints.
    /// </summary>
    public ServiceEndpointsConfig AssignmentGradeServices { get; set; } = new();
}