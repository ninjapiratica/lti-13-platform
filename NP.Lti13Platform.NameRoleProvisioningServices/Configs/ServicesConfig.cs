namespace NP.Lti13Platform.NameRoleProvisioningServices.Configs;

/// <summary>
/// Represents the configuration for name and role provisioning services.
/// </summary>
public record ServicesConfig
{
    /// <summary>
    /// Gets or sets the base service address for name and role provisioning services.
    /// </summary>
    public Uri ServiceAddress { get; set; } = DefaultUri;

    /// <summary>
    /// Gets or sets a value indicating whether to support membership differences.
    /// </summary>
    public bool SupportMembershipDifferences { get; set; } = true;

    /// <summary>
    /// The default URI to use when no service address is provided.
    /// </summary>
    internal readonly static Uri DefaultUri = new("x://x.x.x");
}
