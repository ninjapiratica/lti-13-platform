using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NP.Lti13Platform.Core.MessageHandlers;
using NP.Lti13Platform.Core.Services;
using NP.Lti13Platform.DeepLinking.Configs;
using NP.Lti13Platform.DeepLinking.MessageHandlers;
using NP.Lti13Platform.DeepLinking.Services;

namespace NP.Lti13Platform.DeepLinking;

/// <summary>
/// Provides extension methods to configure LTI 1.3 Deep Linking in an application.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds LTI 1.3 platform deep linking services and configuration to the specified service collection.
    /// </summary>
    /// <remarks>This method registers the required services and binds configuration for LTI 1.3 deep linking support.
    /// It should be called during application startup when configuring dependency injection for an LTI 1.3 platform implementation.</remarks>
    /// <param name="serviceCollection">The service collection to which the LTI 1.3 deep linking services and configuration will be added. Cannot be null.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the ILti13DeepLinkingResponseDataService implementation. The default is ServiceLifetime.Transient.</param>
    /// <returns>The same instance of <see cref="IServiceCollection"/> that was provided, to support method chaining.</returns>
    public static IServiceCollection AddLti13PlatformDeepLinking<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13DeepLinkingResponseDataService
    {
        serviceCollection.AddOptions<DeepLinkingConfig>().BindConfiguration("Lti13Platform:DeepLinking");
        serviceCollection.TryAddSingleton<ILti13DeepLinkingConfigService, DefaultDeepLinkingConfigService>();

        serviceCollection.TryAddSingleton<ILti13DeepLinkingResponseHandler, DefaultDeepLinkingResponseHandler>();

        serviceCollection.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingResponseDataService), typeof(T), serviceLifetime));

        return serviceCollection;
    }

    /// <summary>
    /// Adds an implementation of the ILti13DeepLinkingConfigService interface to the service collection with the specified service lifetime.
    /// </summary>
    /// <remarks>Use this method to register a custom implementation of ILti13DeepLinkingConfigService for dependency injection.
    /// The specified service lifetime determines how the service is instantiated and reused within the application.</remarks>
    /// <typeparam name="T">The type that implements ILti13DeepLinkingConfigService to be registered.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the ILti13DeepLinkingConfigService implementation is added.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service. The default is ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the service registration added. This enables method chaining.</returns>
    public static IServiceCollection WithLti13DeepLinkingConfigService<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13DeepLinkingConfigService
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingConfigService), typeof(T), serviceLifetime));
        return serviceCollection;
    }

    /// <summary>
    /// Registers a custom implementation of the ILti13DeepLinkingResponseHandler interface in the dependency injection container.
    /// </summary>
    /// <remarks>Use this method to configure a specific ILti13DeepLinkingResponseHandler implementation for LTI 1.3 deep linking scenarios.
    /// Only one implementation should be registered at a time to avoid ambiguity during resolution.</remarks>
    /// <typeparam name="T">The type that implements ILti13DeepLinkingResponseHandler to be registered.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the handler implementation will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the handler implementation. The default is ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the handler registration added.</returns>
    public static IServiceCollection WithLti13DeepLinkingResponseHandler<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13DeepLinkingResponseHandler
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingResponseHandler), typeof(T), serviceLifetime));
        return serviceCollection;
    }

    /// <summary>
    /// Registers the default deep linking request message handler and associates the specified deep linking data service implementation with the resource link message data service in the dependency injection container.
    /// </summary>
    /// <remarks>This method registers <see cref="Lti13DeepLinkingRequestMessageHandler"/> as the handler for deep linking request messages
    /// and associates the  <typeparamref name="T"/> implementation with <see cref="ILti13ResourceLinkMessageDataService"/>.
    /// It also registers the handler for both <see cref="ILti13DeepLinkingRequestMessageHandler"/> and <see cref="ILtiMessageHandler"/> interfaces.
    /// Use this method to enable default deep linking support in an LTI 1.3 integration.</remarks>
    /// <typeparam name="T">The type that implements the deep linking data service interface used for resource link message data operations.</typeparam>
    /// <param name="serviceCollection">The dependency injection service collection to which the deep linking message handler and data service will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which the deep linking data service implementation is registered. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, enabling method chaining.</returns>
    public static IServiceCollection WithDefaultLti13DeepLinkingRequestMessageHandler<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13DeepLinkingRequestDataService
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingRequestDataService), typeof(T), serviceLifetime));
        serviceCollection.AddTransient<ILti13DeepLinkingRequestMessageHandler, Lti13DeepLinkingRequestMessageHandler>();
        serviceCollection.AddTransient<ILtiMessageHandler, Lti13DeepLinkingRequestMessageHandler>();
        return serviceCollection;
    }

    /// <summary>
    /// Registers an implementation of the ILti13DeepLinkingMessageExtension interface using the specified resource link message extension type and service lifetime.
    /// </summary>
    /// <remarks>Use this method to enable LTI deep linking message extension support by registering a custom implementation.
    /// This is typically called during application startup as part of dependency injection configuration.</remarks>
    /// <typeparam name="T">The type that implements ILti13DeepLinkingMessageExtension to be used as the deep linking message extension.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the deep linking message extension service will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which the service will be registered. Defaults to ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the deep linking message extension service registered.</returns>
    public static IServiceCollection WithLti13DeepLinkingMessageExtension<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13DeepLinkingMessageExtension
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingMessageExtension), typeof(T), serviceLifetime));
        return serviceCollection;
    }
}
