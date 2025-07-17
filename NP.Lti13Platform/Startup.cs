using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NP.Lti13Platform.AssignmentGradeServices;
using NP.Lti13Platform.AssignmentGradeServices.Configs;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.DeepLinking;
using NP.Lti13Platform.DeepLinking.Configs;
using NP.Lti13Platform.NameRoleProvisioningServices;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;

namespace NP.Lti13Platform;

/// <summary>
/// Provides extension methods for configuring and using LTI 1.3 platform services within an application.
/// </summary>
/// <remarks>
/// This static class includes methods to add, configure, and use LTI 1.3 platform services, such as core services, deep linking, name and role provisioning, and assignment and grade services. These methods extend the <see cref="IServiceCollection"/> and <see cref="IEndpointRouteBuilder"/> interfaces to integrate LTI 1.3 functionalityinto an application.
/// </remarks>
public static class Startup
{
    public static Lti13PlatformBuilder AddLti13Platform(this IServiceCollection services)
    {
        return services
            .AddLti13PlatformCore()
            .AddLti13PlatformDeepLinking()
            .AddLti13PlatformNameRoleProvisioningServices()
            .AddLti13PlatformAssignmentGradeServices();
    }

    public static Lti13PlatformBuilder WithLti13DataService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13DataService
    {
        builder.WithLti13CoreDataService<T>(serviceLifetime)
            .WithLti13DeepLinkingDataService<T>(serviceLifetime)
            .WithLti13NameRoleProvisioningDataService<T>(serviceLifetime)
            .WithLti13AssignmentGradeDataService<T>(serviceLifetime);

        return builder;
    }

    public static IEndpointRouteBuilder UseLti13Platform(this IEndpointRouteBuilder app, Func<Lti13PlatformEndpointsConfig, Lti13PlatformEndpointsConfig>? configure = null, string openAPIGroupName = "")
    {
        Lti13PlatformEndpointsConfig config = new();
        config = configure?.Invoke(config) ?? config;

        return app
            .UseLti13PlatformCore(x => config.Core, openAPIGroupName)
            .UseLti13PlatformDeepLinking(x => config.DeepLinking, openAPIGroupName)
            .UseLti13PlatformNameRoleProvisioningServices(x => config.NameRoleProvisioningServices, openAPIGroupName)
            .UseLti13PlatformAssignmentGradeServices(x => config.AssignmentGradeServices, openAPIGroupName);
    }
}

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