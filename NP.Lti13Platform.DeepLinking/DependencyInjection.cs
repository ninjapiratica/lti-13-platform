using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.MessageHandlers;
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
    /// Adds LTI 1.3 Deep Linking support to the builder and registers the specified deep linking response data service
    /// with the given service lifetime.
    /// </summary>
    /// <remarks>This method configures required options and services for LTI 1.3 Deep Linking, including
    /// binding deep linking configuration from the application's configuration sources. It also registers default
    /// implementations for core deep linking services if they are not already registered. Call this method during
    /// application startup to enable Deep Linking support in your LTI 1.3 integration.</remarks>
    /// <typeparam name="TService">The type of the deep linking response data service to register. Must implement <see
    /// cref="ILti13DeepLinkingResponseDataService"/>.</typeparam>
    /// <param name="builder">The core builder to extend with deep linking services.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the deep linking response data service. The default is <see
    /// cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The builder instance, enabling fluent configuration of additional services.</returns>
    public static ILti13CoreBuilder AddDeepLinking<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILti13DeepLinkingResponseDataService
    {
        builder.Services.AddOptions<DeepLinkingConfig>().BindConfiguration("Lti13Platform:DeepLinking");
        builder.Services.TryAddSingleton<ILti13DeepLinkingConfigService, DefaultDeepLinkingConfigService>();

        builder.Services.TryAddSingleton<ILti13DeepLinkingResponseHandler, DefaultDeepLinkingResponseHandler>();

        builder.Services.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingResponseDataService), typeof(TService), serviceLifetime));

        return builder;
    }

    /// <summary>
    /// Registers an implementation of IDeepLinkingConfigService with the specified service lifetime.
    /// </summary>
    /// <typeparam name="TService">The type that implements ILti13DeepLinkingConfigService.</typeparam>
    /// <param name="builder">The core builder to extend.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service.</param>
    /// <returns>The same builder instance, enabling fluent method chaining.</returns>
    public static ILti13CoreBuilder WithDeepLinkingConfigService<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILti13DeepLinkingConfigService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingConfigService), typeof(TService), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Registers an implementation of IDeepLinkingResponseHandler with the specified service lifetime.
    /// </summary>
    /// <typeparam name="TService">The type that implements ILti13DeepLinkingResponseHandler.</typeparam>
    /// <param name="builder">The core builder to extend.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service.</param>
    /// <returns>The same builder instance, enabling fluent method chaining.</returns>
    public static ILti13CoreBuilder WithDeepLinkingResponseHandler<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILti13DeepLinkingResponseHandler
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingResponseHandler), typeof(TService), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Registers the default deep linking request message handler with the specified service lifetime.
    /// </summary>
    /// <typeparam name="TService">The type that implements ILti13DeepLinkingRequestDataService.</typeparam>
    /// <param name="builder">The core builder to extend.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service.</param>
    /// <returns>The same builder instance, enabling fluent method chaining.</returns>
    public static ILti13CoreBuilder WithDefaultDeepLinkingRequestMessageHandler<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILti13DeepLinkingRequestDataService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingRequestDataService), typeof(TService), serviceLifetime));
        builder.Services.AddTransient<IDeepLinkingRequestMessageHandler, DeepLinkingRequestMessageHandler>();
        builder.Services.AddTransient<ILti13MessageHandler, DeepLinkingRequestMessageHandler>();
        return builder;
    }

    /// <summary>
    /// Registers an implementation of IDeepLinkingMessageExtension with the specified service lifetime.
    /// </summary>
    /// <typeparam name="TService">The type that implements ILti13DeepLinkingMessageExtension.</typeparam>
    /// <param name="builder">The core builder to extend.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service.</param>
    /// <returns>The same builder instance, enabling fluent method chaining.</returns>
    public static ILti13CoreBuilder WithDeepLinkingMessageExtension<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILti13DeepLinkingMessageExtension
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13DeepLinkingMessageExtension), typeof(TService), serviceLifetime));
        return builder;
    }
}
