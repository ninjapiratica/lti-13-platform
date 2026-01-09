using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.Core.MessageHandlers;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;

namespace NP.Lti13Platform.Core;

/// <summary>
/// Provides extension methods for configuring and using LTI 1.3 platform core services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds core LTI 1.3 platform services and configuration to the specified service collection.
    /// </summary>
    /// <remarks>This method registers authentication, configuration, and singleton services required for LTI 1.3 platform support.
    /// It should be called during application startup to ensure all necessary dependencies are available for LTI 1.3 operations.</remarks>
    /// <param name="serviceCollection">The service collection to which the LTI 1.3 platform services will be added. Cannot be null.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the ILti13CoreDataService implementation. The default is ServiceLifetime.Transient.</param>
    /// <returns>The same instance of <see cref="IServiceCollection"/> that was provided, to support method chaining.</returns>
    public static IServiceCollection AddLti13PlatformCore<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13CoreDataService
    {
        serviceCollection.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, LtiServicesAuthHandler>(LtiServicesAuthHandler.SchemeName, null);

        serviceCollection.AddHttpContextAccessor();

        serviceCollection.AddOptions<Platform>().BindConfiguration("Lti13Platform:Platform");
        serviceCollection.TryAddSingleton<ILti13PlatformService, DefaultLti13PlatformService>();

        serviceCollection.AddOptions<Lti13PlatformTokenConfig>()
            .BindConfiguration("Lti13Platform:Token")
            .Validate(x => x.Issuer.Scheme == Uri.UriSchemeHttps, "Lti13Platform:Token:Issuer is required when using default ILti13TokenConfigService.");
        serviceCollection.TryAddSingleton<ILti13TokenConfigService, DefaultLti13TokenConfigService>();

        serviceCollection.TryAddSingleton<ILti13ToolSecurityService, DefaultLti13ToolSecurityService>();

        serviceCollection.Add(new ServiceDescriptor(typeof(ILti13CoreDataService), typeof(T), serviceLifetime));

        return serviceCollection;
    }

    /// <summary>
    /// Registers an implementation of the ILti13PlatformService interface in the service collection with the specified service lifetime.
    /// </summary>
    /// <remarks>Use this method to add a custom implementation of ILti13PlatformService to the dependency injection container.
    /// This enables dependency injection of ILti13PlatformService throughout the application.</remarks>
    /// <typeparam name="T">The type that implements ILti13PlatformService to be registered.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the ILti13PlatformService implementation will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service. The default is ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the ILti13PlatformService implementation registered.</returns>
    public static IServiceCollection WithLti13PlatformService<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13PlatformService
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ILti13PlatformService), typeof(T), serviceLifetime));
        return serviceCollection;
    }

    /// <summary>
    /// Registers an implementation of the ILti13TokenConfigService interface in the service collection with the specified service lifetime.
    /// </summary>
    /// <remarks>Use this method to configure dependency injection for LTI 1.3 token configuration services. 
    /// This enables the application to resolve ILti13TokenConfigService dependencies using the specified implementation and lifetime.</remarks>
    /// <typeparam name="T">The type that implements ILti13TokenConfigService to be registered.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the ILti13TokenConfigService implementation is added.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service. The default is ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance for chaining further service registrations.</returns>
    public static IServiceCollection WithLti13TokenConfigService<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13TokenConfigService
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ILti13TokenConfigService), typeof(T), serviceLifetime));
        return serviceCollection;
    }

    /// <summary>
    /// Registers the specified message handler type as an implementation of ILtiMessageHandler in the service collection.
    /// </summary>
    /// <remarks>This method enables dependency injection of a custom ILtiMessageHandler implementation.
    /// Use this method to configure the desired handler and its lifetime when setting up services for LTI message processing.</remarks>
    /// <typeparam name="T">The type of the message handler to register. Must implement ILtiMessageHandler.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the message handler will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the message handler. The default is ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the message handler registration added.</returns>
    public static IServiceCollection WithMessageHandler<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILtiMessageHandler
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ILtiMessageHandler), typeof(T), serviceLifetime));
        return serviceCollection;
    }

    /// <summary>
    /// Registers an implementation of the ILti13ResourceLinkMessageDataService interface and the Lti13ResourceLinkRequestMessageHandler for handling LTI 1.3 resource link messages in the dependency injection container.
    /// </summary>
    /// <remarks>This method enables LTI 1.3 resource link message handling by registering the required services.
    /// Call this method during application startup to ensure that LTI resource link requests are processed correctly.</remarks>
    /// <typeparam name="T">The type that implements ILti13ResourceLinkMessageDataService to be registered.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the services are added.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the ILti13ResourceLinkMessageDataService implementation. The default is ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the LTI resource link message handler services registered.</returns>
    public static IServiceCollection WithDefaultLti13ResourceLinkMessageHandler<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13ResourceLinkMessageDataService
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ILti13ResourceLinkMessageDataService), typeof(T), serviceLifetime));
        serviceCollection.AddTransient<ILti13ResourceLinkRequestMessageHandler, Lti13ResourceLinkRequestMessageHandler>();
        serviceCollection.AddTransient<ILtiMessageHandler, Lti13ResourceLinkRequestMessageHandler>();
        return serviceCollection;
    }

    /// <summary>
    /// Registers an implementation of the ILti13ResourceLinkMessageExtension interface in the service collection with the specified service lifetime.
    /// </summary>
    /// <remarks>Use this method to enable dependency injection for LTI Resource Link Message extensions.
    /// This allows consumers to resolve ILti13ResourceLinkMessageExtension implementations from the service provider according to the specified lifetime.</remarks>
    /// <typeparam name="T">The type that implements ILti13ResourceLinkMessageExtension to be registered.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the ILti13ResourceLinkMessageExtension implementation will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which the ILti13ResourceLinkMessageExtension service will be registered. Defaults to ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the ILti13ResourceLinkMessageExtension service registration added.</returns>
    public static IServiceCollection WithLti13ResourceLinkMessageExtension<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13ResourceLinkMessageExtension
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ILti13ResourceLinkMessageExtension), typeof(T), serviceLifetime));
        return serviceCollection;
    }
}