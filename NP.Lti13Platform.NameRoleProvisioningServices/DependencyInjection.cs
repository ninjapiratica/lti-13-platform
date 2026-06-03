using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;
using NP.Lti13Platform.NameRoleProvisioningServices.MessageClaims;
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
    /// <remarks>This method registers the default implementation for <see cref="ILti13NameRoleProvisioningDataService"/> and binds configuration from the 'Lti13Platform:NameRoleProvisioningServices' section.
    /// Call this method during application startup to enable LTI 1.3 Name/Role Provisioning support.</remarks>
    /// <typeparam name="TService">The type that implements the name/role provisioning data service interface.</typeparam>
    /// <param name="builder">The core builder to extend with name/role provisioning services.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the services.</param>
    /// <returns>The builder instance, enabling fluent configuration of additional services.</returns>
    public static ILti13CoreBuilder AddNameRoleProvisioningServices<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILti13NameRoleProvisioningDataService
    {
        builder.WithResourceLinkMessageExtension<NameRoleServiceMessageExtension>();

        builder.Services.AddOptions<ServicesConfig>().BindConfiguration("Lti13Platform:NameRoleProvisioningServices");
        builder.Services.TryAddSingleton<ILti13NameRoleProvisioningConfigService, DefaultNameRoleProvisioningConfigService>();

        builder.Services.Add(new ServiceDescriptor(typeof(ILti13NameRoleProvisioningDataService), typeof(TService), serviceLifetime));

        return builder;
    }

    /// <summary>
    /// Registers an implementation of INameRoleProvisioningConfigService with the specified service lifetime.
    /// </summary>
    /// <typeparam name="TService">The type that implements ILti13NameRoleProvisioningConfigService.</typeparam>
    /// <param name="builder">The core builder to extend.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service.</param>
    /// <returns>The same builder instance, enabling fluent method chaining.</returns>
    public static ILti13CoreBuilder WithNameRoleProvisioningConfigService<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILti13NameRoleProvisioningConfigService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13NameRoleProvisioningConfigService), typeof(TService), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Registers an implementation of INameRoleProvisioningServicesMessageExtension with the specified service lifetime.
    /// </summary>
    /// <typeparam name="TService">The type that implements ILti13NameRoleProvisioningServicesMessageExtension.</typeparam>
    /// <param name="builder">The core builder to extend.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service.</param>
    /// <returns>The same builder instance, enabling fluent method chaining.</returns>
    public static ILti13CoreBuilder WithNameRoleProvisioningServicesMessageExtension<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILti13NameRoleProvisioningServicesMessageExtension
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13NameRoleProvisioningServicesMessageExtension), typeof(TService), serviceLifetime));
        return builder;
    }
}
