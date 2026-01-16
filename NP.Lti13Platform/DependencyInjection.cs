using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NP.Lti13Platform.AssignmentGradeServices;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.DeepLinking;
using NP.Lti13Platform.NameRoleProvisioningServices;

namespace NP.Lti13Platform;

/// <summary>
/// Provides extension methods for configuring and using LTI 1.3 platform services within an application.
/// </summary>
/// <remarks>
/// This static class includes methods to add, configure, and use LTI 1.3 platform services, such as core services, deep linking, name and role provisioning, and assignment and grade services. These methods extend the <see cref="IServiceCollection"/> and <see cref="IEndpointRouteBuilder"/> interfaces to integrate LTI 1.3 functionality into an application.
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Adds LTI 1.3 platform services, including core, deep linking, names and roles provisioning, and assignment and grade services, to the specified service collection.
    /// </summary>
    /// <remarks>This method registers all standard LTI 1.3 platform services required for typical integration scenarios. Call this method during application startup to enable LTI 1.3 support.</remarks>
    /// <typeparam name="T">The type that implements the required data service for LTI 1.3 platform operations.</typeparam>
    /// <param name="services">The service collection to which the LTI 1.3 platform services will be added.</param>
    /// <returns>A unified platform builder instance that enables fluent configuration of all LTI 1.3 platform services.</returns>
    public static ILti13CoreBuilder AddLti13Platform<T>(this IServiceCollection services)
        where T : ILti13PlatformDataService
    {
        return services.AddLti13PlatformCore<T>()
            .AddDeepLinking<T>()
            .AddNameRoleProvisioningServices<T>()
            .AddAssignmentGradeServices<T>();
    }

    /// <summary>
    /// Adds LTI 1.3 platform services with default message handlers to the service collection.
    /// </summary>
    /// <remarks>This method registers all standard LTI 1.3 platform services including the resource link and deep linking message handlers.
    /// Call this method during application startup to enable full LTI 1.3 support.</remarks>
    /// <typeparam name="T">The type that implements the required data service for LTI 1.3 platform operations.</typeparam>
    /// <param name="services">The service collection to add the LTI 1.3 platform services to.</param>
    /// <returns>A unified platform builder instance that enables fluent configuration of all LTI 1.3 platform services with default message handlers.</returns>
    public static ILti13CoreBuilder AddLti13PlatformWithDefaultMessageHandlers<T>(this IServiceCollection services)
        where T : ILti13PlatformDataServiceWithDefaultHandlers
    {
        return services.AddLti13PlatformCore<T>()
            .AddDeepLinking<T>()
            .AddNameRoleProvisioningServices<T>()
            .AddAssignmentGradeServices<T>()
            .WithDefaultLtiResourceLinkMessageHandler<T>()
            .WithDefaultDeepLinkingRequestMessageHandler<T>();
    }
}