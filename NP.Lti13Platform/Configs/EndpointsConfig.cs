namespace NP.Lti13Platform.Configs;

/// <summary>
/// Represents the configuration for LTI 1.3 platform endpoints.
/// </summary>
/// <remarks>
/// This class provides access to various endpoint configurations used in LTI 1.3 integrations, including core endpoints, deep linking, name and role provisioning services, and assignment and grade services.
/// </remarks>
public class EndpointsConfig
{
    /// <summary>
    /// Gets or sets the configuration for core LTI 1.3 platform endpoints.
    /// </summary>
    public Core.Configs.EndpointsConfig Core { get; set; } = new();

    /// <summary>
    /// Gets or sets the configuration for LTI 1.3 deep linking endpoints.
    /// </summary>
    public DeepLinking.Configs.EndpointsConfig DeepLinking { get; set; } = new();

    /// <summary>
    /// Gets or sets the configuration for LTI 1.3 name and role provisioning services endpoints.
    /// </summary>
    public NameRoleProvisioningServices.Configs.EndpointsConfig NameRoleProvisioningServices { get; set; } = new();

    /// <summary>
    /// Gets or sets the configuration for LTI 1.3 assignment and grade services endpoints.
    /// </summary>
    public AssignmentGradeServices.Configs.EndpointsConfig AssignmentGradeServices { get; set; } = new();
}