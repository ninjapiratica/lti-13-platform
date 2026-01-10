using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NP.Lti13Platform.AssignmentGradeServices;
using NP.Lti13Platform.Configs;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.DeepLinking;
using NP.Lti13Platform.NameRoleProvisioningServices;

namespace NP.Lti13Platform;

/// <summary>
/// Provides extension methods for configuring and using LTI 1.3 platform services within an application.
/// </summary>
/// <remarks>
/// This static class includes methods to add, configure, and use LTI 1.3 platform services, such as core services, deep linking, name and role provisioning, and assignment and grade services. These methods extend the <see cref="IServiceCollection"/> and <see cref="IEndpointRouteBuilder"/> interfaces to integrate LTI 1.3 functionalityinto an application.
/// </remarks>
public static class Endpoints
{
    /// <summary>
    /// Configures LTI 1.3 platform endpoints in the application's request processing pipeline.
    /// </summary>
    /// <param name="app">The endpoint route builder to configure the LTI 1.3 platform endpoints for.</param>
    /// <param name="configure">An optional function to configure the LTI 1.3 platform endpoints.</param>
    /// <returns>The configured endpoint route builder.</returns>
    public static IEndpointRouteBuilder UseLti13Platform(this IEndpointRouteBuilder app, Func<EndpointsConfig, EndpointsConfig>? configure = null)
    {
        EndpointsConfig config = new();
        config = configure?.Invoke(config) ?? config;

        return app
            .UseLti13PlatformCore(x => config.Core)
            .UseLti13PlatformDeepLinking(x => config.DeepLinking)
            .UseLti13PlatformNameRoleProvisioningServices(x => config.NameRoleProvisioningServices)
            .UseLti13PlatformAssignmentGradeServices(x => config.AssignmentGradeServices);
    }
}
