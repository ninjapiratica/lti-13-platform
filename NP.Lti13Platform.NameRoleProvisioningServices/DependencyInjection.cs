using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;
using NP.Lti13Platform.NameRoleProvisioningServices.MessageHandlers;
using NP.Lti13Platform.NameRoleProvisioningServices.Services;

namespace NP.Lti13Platform.NameRoleProvisioningServices;

/// <summary>
/// Provides extension methods for configuring LTI 1.3 Name and Role Provisioning Services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds LTI 1.3 platform Name/Role Provisioning services and related configuration to the service collection.
    /// </summary>
    /// <remarks>This method registers the default implementation for <see cref="ILti13NameRoleProvisioningConfigService"/> and binds configuration from the 'Lti13Platform:NameRoleProvisioningServices' section.
    /// Call this method during application startup to enable LTI 1.3 Name/Role Provisioning support.</remarks>
    /// <param name="serviceCollection">The service collection to which the Name/Role Provisioning services will be added. Cannot be null.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the services.</param>
    /// <returns>The same instance of <see cref="IServiceCollection"/> that was provided, to support method chaining.</returns>
    public static IServiceCollection AddLti13PlatformNameRoleProvisioningServices<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13NameRoleProvisioningDataService
    {
        serviceCollection.AddOptions<ServicesConfig>().BindConfiguration("Lti13Platform:NameRoleProvisioningServices");
        serviceCollection.TryAddSingleton<ILti13NameRoleProvisioningConfigService, DefaultLti13NameRoleProvisioningConfigService>();

        serviceCollection.Add(new ServiceDescriptor(typeof(ILti13NameRoleProvisioningDataService), typeof(T), serviceLifetime));

        return serviceCollection;
    }

    /// <summary>
    /// Adds an implementation of ILti13NameRoleProvisioningConfigService to the service collection with the specified service lifetime.
    /// </summary>
    /// <remarks>Use this method to register a custom implementation of ILti13NameRoleProvisioningConfigService for dependency injection.
    /// This enables the application to resolve ILti13NameRoleProvisioningConfigService using the specified implementation and lifetime.</remarks>
    /// <typeparam name="T">The type that implements ILti13NameRoleProvisioningConfigService to register.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the service will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service. The default is ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the service registration added.</returns>
    public static IServiceCollection WithLti13NameRoleProvisioningConfigService<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13NameRoleProvisioningConfigService
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ILti13NameRoleProvisioningConfigService), typeof(T), serviceLifetime));
        return serviceCollection;
    }

    /// <summary>
    /// Adds an implementation of the ILtiNameRoleProvisioningServicesMessageExtension interface to the service collection with the specified service lifetime.
    /// </summary>
    /// <remarks>Use this method to enable LTI Name and Role Provisioning Services message extension support in your application's dependency injection container.
    /// This allows the application to resolve ILtiNameRoleProvisioningServicesMessageExtension implementations as needed.</remarks>
    /// <typeparam name="T">The type that implements ILtiNameRoleProvisioningServicesMessageExtension to register.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the message extension implementation will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service. The default is ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the message extension service registered.</returns>
    public static IServiceCollection WithLti13NameRoleProvisioningServicesMessageExtension<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13NameRoleProvisioningServicesMessageExtension
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ILti13NameRoleProvisioningServicesMessageExtension), typeof(T), serviceLifetime));
        return serviceCollection;
    }
}