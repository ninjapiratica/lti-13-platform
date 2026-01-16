using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.Core.MessageHandlers;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;

namespace NP.Lti13Platform.Core;

/// <summary>
/// Provides a builder interface for fluently configuring LTI 1.3 platform core services.
/// </summary>
public interface ILti13CoreBuilder
{
    /// <summary>
    /// Gets the underlying service collection for additional configuration.
    /// </summary>
    IServiceCollection Services { get; }
}

/// <summary>
/// Default implementation of IPlatformCoreBuilder.
/// </summary>
/// <remarks>
/// Initializes a new instance of the PlatformCoreBuilder class.
/// </remarks>
internal sealed class Lti13PlatformCoreBuilder(IServiceCollection services)
    : ILti13CoreBuilder
{
    public IServiceCollection Services => services;
}

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
    /// <returns>A builder instance that enables fluent configuration of additional core services.</returns>
    public static ILti13CoreBuilder AddLti13PlatformCore<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : ILti13CoreDataService
    {
        var builder = new Lti13PlatformCoreBuilder(serviceCollection);

        builder.Services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, ServicesAuthHandler>(ServicesAuthHandler.SchemeName, null);

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddOptions<Platform>().BindConfiguration("Lti13Platform:Platform");
        builder.Services.TryAddSingleton<ILti13PlatformService, DefaultPlatformService>();

        builder.Services.AddOptions<TokenConfig>()
            .BindConfiguration("Lti13Platform:Token")
            .Validate(x => x.Issuer.Scheme == Uri.UriSchemeHttps, "Lti13Platform:Token:Issuer is required when using default ITokenConfigService.");
        builder.Services.TryAddSingleton<ILti13TokenConfigService, DefaultTokenConfigService>();

        builder.Services.TryAddSingleton<IToolSecurityService, DefaultToolSecurityService>();

        builder.Services.Add(new ServiceDescriptor(typeof(ILti13CoreDataService), typeof(T), serviceLifetime));

        return builder;
    }

    /// <summary>
    /// Registers an implementation of IPlatformService with the specified service lifetime.
    /// </summary>
    /// <typeparam name="TService">The type that implements ILti13PlatformService.</typeparam>
    /// <param name="builder">The core builder to extend.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service.</param>
    /// <returns>The same builder instance, enabling fluent method chaining.</returns>
    public static ILti13CoreBuilder WithPlatformService<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILti13PlatformService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13PlatformService), typeof(TService), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Registers an implementation of ITokenConfigService with the specified service lifetime.
    /// </summary>
    /// <typeparam name="TService">The type that implements ILti13TokenConfigService.</typeparam>
    /// <param name="builder">The core builder to extend.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service.</param>
    /// <returns>The same builder instance, enabling fluent method chaining.</returns>
    public static ILti13CoreBuilder WithTokenConfigService<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILti13TokenConfigService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13TokenConfigService), typeof(TService), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Registers an implementation of IMessageHandler with the specified service lifetime.
    /// </summary>
    /// <typeparam name="TService">The type that implements ILti13MessageHandler.</typeparam>
    /// <param name="builder">The core builder to extend.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service.</param>
    /// <returns>The same builder instance, enabling fluent method chaining.</returns>
    public static ILti13CoreBuilder WithMessageHandler<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILti13MessageHandler
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13MessageHandler), typeof(TService), serviceLifetime));
        return builder;
    }

    /// <summary>
    /// Registers the default LTI resource link message handler with the specified service lifetime.
    /// </summary>
    /// <typeparam name="TService">The type that implements ILtiResourceLinkMessageDataService.</typeparam>
    /// <param name="builder">The core builder to extend.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service.</param>
    /// <returns>The same builder instance, enabling fluent method chaining.</returns>
    public static ILti13CoreBuilder WithDefaultLtiResourceLinkMessageHandler<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILtiResourceLinkMessageDataService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILtiResourceLinkMessageDataService), typeof(TService), serviceLifetime));
        builder.Services.AddTransient<ILtiResourceLinkRequestMessageHandler, LtiResourceLinkRequestMessageHandler>();
        builder.Services.AddTransient<ILti13MessageHandler, LtiResourceLinkRequestMessageHandler>();
        return builder;
    }

    /// <summary>
    /// Registers an implementation of IResourceLinkMessageExtension with the specified service lifetime.
    /// </summary>
    /// <typeparam name="TService">The type that implements ILtiResourceLinkMessageExtension.</typeparam>
    /// <param name="builder">The core builder to extend.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service.</param>
    /// <returns>The same builder instance, enabling fluent method chaining.</returns>
    public static ILti13CoreBuilder WithResourceLinkMessageExtension<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILtiResourceLinkMessageExtension
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILtiResourceLinkMessageExtension), typeof(TService), serviceLifetime));
        return builder;
    }
}
