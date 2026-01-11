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
/// This static class includes methods to add, configure, and use LTI 1.3 platform services, such as core services, deep linking, name and role provisioning, and assignment and grade services. These methods extend the <see cref="IServiceCollection"/> and <see cref="IEndpointRouteBuilder"/> interfaces to integrate LTI 1.3 functionalityinto an application.
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Adds LTI 1.3 platform services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the LTI 1.3 platform services to.</param>
    /// <returns>A builder object that allows further configuration of the LTI 1.3 platform services.</returns>
    public static IServiceCollection AddPlatformWithDefaultMessageHandlers<T>(this IServiceCollection services)
        where T : IDataService
    {
        return services
            .AddPlatform<T>()
            .WithPlatformDefaultMessageHandlers<T>();
    }

    /// <summary>
    /// Adds LTI 1.3 platform services, including core, deep linking, names and roles provisioning, and assignment and grade services, to the specified service collection.
    /// </summary>
    /// <remarks>This method registers all standard LTI 1.3 platform services required for typical integration scenarios. Call this method during application startup to enable LTI 1.3 support.</remarks>
    /// <typeparam name="T">The type that implements the required data service for LTI 1.3 platform operations.</typeparam>
    /// <param name="services">The service collection to which the LTI 1.3 platform services will be added.</param>
    /// <returns>The service collection with the LTI 1.3 platform services registered. This enables further configuration or chaining of service registrations.</returns>
    public static IServiceCollection AddPlatform<T>(this IServiceCollection services)
        where T : IRequiredDataService
    {
        return services
            .AddPlatformCore<T>()
            .AddPlatformDeepLinking<T>()
            .AddPlatformNameRoleProvisioningServices<T>()
            .AddPlatformAssignmentGradeServices<T>();
    }

    /// <summary>
    /// Registers the default LTI 1.3 platform message handlers for resource link and deep linking requests using the specified data service type.
    /// </summary>
    /// <remarks>This method is typically called during application startup to configure LTI 1.3 message handling for an ASP.NET Core application.
    /// It adds both the resource link and deep linking request handlers using the specified data service type.</remarks>
    /// <typeparam name="T">The type of the data service to use for LTI 1.3 message handling. Must implement the IMessageHandlerDataService interface.</typeparam>
    /// <param name="services">The service collection to which the LTI 1.3 platform message handlers will be added.</param>
    /// <returns>The IServiceCollection instance with the default LTI 1.3 platform message handlers registered.</returns>
    public static IServiceCollection WithPlatformDefaultMessageHandlers<T>(this IServiceCollection services)
        where T : IMessageHandlerDataService
    {
        return services
            .WithDefaultLtiResourceLinkMessageHandler<T>()
            .WithDefaultDeepLinkingRequestMessageHandler<T>();
    }
}