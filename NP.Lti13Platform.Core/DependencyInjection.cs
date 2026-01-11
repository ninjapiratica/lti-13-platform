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
    /// <param name="serviceLifetime">The lifetime with which to register the ICoreDataService implementation. The default is ServiceLifetime.Transient.</param>
    /// <returns>The same instance of <see cref="IServiceCollection"/> that was provided, to support method chaining.</returns>
    public static IServiceCollection AddPlatformCore<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ICoreDataService
    {
        serviceCollection.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, ServicesAuthHandler>(ServicesAuthHandler.SchemeName, null);

        serviceCollection.AddHttpContextAccessor();

        serviceCollection.AddOptions<Platform>().BindConfiguration("Lti13Platform:Platform");
        serviceCollection.TryAddSingleton<IPlatformService, DefaultPlatformService>();

        serviceCollection.AddOptions<TokenConfig>()
            .BindConfiguration("Lti13Platform:Token")
            .Validate(x => x.Issuer.Scheme == Uri.UriSchemeHttps, "Lti13Platform:Token:Issuer is required when using default ITokenConfigService.");
        serviceCollection.TryAddSingleton<ITokenConfigService, DefaultTokenConfigService>();

        serviceCollection.TryAddSingleton<IToolSecurityService, DefaultToolSecurityService>();

        serviceCollection.Add(new ServiceDescriptor(typeof(ICoreDataService), typeof(T), serviceLifetime));

        return serviceCollection;
    }

    /// <summary>
    /// Registers an implementation of the IPlatformService interface in the service collection with the specified service lifetime.
    /// </summary>
    /// <remarks>Use this method to add a custom implementation of IPlatformService to the dependency injection container.
    /// This enables dependency injection of IPlatformService throughout the application.</remarks>
    /// <typeparam name="T">The type that implements IPlatformService to be registered.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the IPlatformService implementation will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service. The default is ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the IPlatformService implementation registered.</returns>
    public static IServiceCollection WithPlatformService<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : IPlatformService
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(IPlatformService), typeof(T), serviceLifetime));
        return serviceCollection;
    }

    /// <summary>
    /// Registers an implementation of the ITokenConfigService interface in the service collection with the specified service lifetime.
    /// </summary>
    /// <remarks>Use this method to configure dependency injection for LTI 1.3 token configuration services. 
    /// This enables the application to resolve ITokenConfigService dependencies using the specified implementation and lifetime.</remarks>
    /// <typeparam name="T">The type that implements ITokenConfigService to be registered.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the ITokenConfigService implementation is added.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service. The default is ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance for chaining further service registrations.</returns>
    public static IServiceCollection WithTokenConfigService<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ITokenConfigService
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ITokenConfigService), typeof(T), serviceLifetime));
        return serviceCollection;
    }

    /// <summary>
    /// Registers the specified message handler type as an implementation of IMessageHandler in the service collection.
    /// </summary>
    /// <remarks>This method enables dependency injection of a custom IMessageHandler implementation.
    /// Use this method to configure the desired handler and its lifetime when setting up services for LTI 1.3 message processing.</remarks>
    /// <typeparam name="T">The type of the message handler to register. Must implement IMessageHandler.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the message handler will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the message handler. The default is ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the message handler registration added.</returns>
    public static IServiceCollection WithMessageHandler<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : IMessageHandler
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(IMessageHandler), typeof(T), serviceLifetime));
        return serviceCollection;
    }

    /// <summary>
    /// Registers an implementation of the IResourceLinkMessageDataService interface and the ResourceLinkRequestMessageHandler for handling LTI 1.3 resource link messages in the dependency injection container.
    /// </summary>
    /// <remarks>This method enables LTI 1.3 resource link message handling by registering the required services.
    /// Call this method during application startup to ensure that LTI 1.3 resource link requests are processed correctly.</remarks>
    /// <typeparam name="T">The type that implements IResourceLinkMessageDataService to be registered.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the services are added.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the IResourceLinkMessageDataService implementation. The default is ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the LTI 1.3 resource link message handler services registered.</returns>
    public static IServiceCollection WithDefaultLtiResourceLinkMessageHandler<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILtiResourceLinkMessageDataService
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ILtiResourceLinkMessageDataService), typeof(T), serviceLifetime));
        serviceCollection.AddTransient<ILtiResourceLinkRequestMessageHandler, LtiResourceLinkRequestMessageHandler>();
        serviceCollection.AddTransient<IMessageHandler, LtiResourceLinkRequestMessageHandler>();
        return serviceCollection;
    }

    /// <summary>
    /// Registers an implementation of the IResourceLinkMessageExtension interface in the service collection with the specified service lifetime.
    /// </summary>
    /// <remarks>Use this method to enable dependency injection for LTI 1.3 Resource Link Message extensions.
    /// This allows consumers to resolve IResourceLinkMessageExtension implementations from the service provider according to the specified lifetime.
    /// The implementation should inherit from ILtiResourceLinkMessageExtension&lt;TMessage&gt; where TMessage is an interface extending ILtiResourceLinkRequestMessage.</remarks>
    /// <typeparam name="T">The type that implements IResourceLinkMessageExtension to be registered.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the IResourceLinkMessageExtension implementation will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which the IResourceLinkMessageExtension service will be registered. Defaults to ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the IResourceLinkMessageExtension service registration added.</returns>
    public static IServiceCollection WithResourceLinkMessageExtension<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILtiResourceLinkMessageExtension
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ILtiResourceLinkMessageExtension), typeof(T), serviceLifetime));
        return serviceCollection;
    }
}