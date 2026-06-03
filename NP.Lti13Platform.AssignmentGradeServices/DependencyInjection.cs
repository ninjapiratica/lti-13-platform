using Microsoft.Extensions.DependencyInjection;
using NP.Lti13Platform.AssignmentGradeServices.Configs;
using NP.Lti13Platform.AssignmentGradeServices.MessageClaims;
using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.MessageHandlers;

namespace NP.Lti13Platform.AssignmentGradeServices;

/// <summary>
/// Provides extension methods for registering LTI 1.3 assignment grade services and related configuration services with an application's dependency injection container.
/// </summary>
/// <remarks>These methods are intended to be used during application startup to configure required services for LTI 1.3 assignment and grade integration.
/// They support flexible service lifetimes and allow customization of service implementations.
/// All methods extend IServiceCollection for seamless integration with ASP.NET Core dependency injection.</remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Adds LTI 1.3 platform assignment grade services to the specified builder, including configuration and required dependencies.
    /// </summary>
    /// <remarks>This method configures assignment grade services for LTI 1.3 platform integration, including binding configuration settings and registering required service implementations.
    /// It should be called during application startup to enable assignment grade functionality.</remarks>
    /// <typeparam name="TService">The type that implements the assignment grade data service interface. Must implement <see cref="ILti13AssignmentGradeDataService"/>.</typeparam>
    /// <param name="builder">The platform core builder to which the assignment grade services will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which the assignment grade data service will be registered. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The builder instance, enabling fluent configuration of additional services.</returns>
    public static ILti13CoreBuilder AddAssignmentGradeServices<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILti13AssignmentGradeDataService
    {
        builder.WithResourceLinkMessageExtension<LineItemServiceMessageExtension>();
        builder.WithAssignmentGradeConfigService<DefaultAssignmentGradeConfigService>(ServiceLifetime.Singleton);

        builder.Services.AddOptions<ServicesConfig>().BindConfiguration("Lti13Platform:AssignmentGradeServices");

        builder.Services.Add(new ServiceDescriptor(typeof(ILti13AssignmentGradeDataService), typeof(TService), serviceLifetime));
        builder.Services.Add(new ServiceDescriptor(typeof(ILtiResourceLinkMessageExtension), typeof(LineItemServiceMessageExtension), ServiceLifetime.Transient));

        return builder;
    }

    /// <summary>
    /// Registers an implementation of IAssignmentGradeConfigService with the specified service lifetime.
    /// </summary>
    /// <typeparam name="TService">The type that implements ILti13AssignmentGradeConfigService.</typeparam>
    /// <param name="builder">The core builder to extend.</param>
    /// <param name="serviceLifetime">The lifetime with which to register the service.</param>
    /// <returns>The same builder instance, enabling fluent method chaining.</returns>
    public static ILti13CoreBuilder WithAssignmentGradeConfigService<TService>(this ILti13CoreBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TService : ILti13AssignmentGradeConfigService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13AssignmentGradeConfigService), typeof(TService), serviceLifetime));
        return builder;
    }
}
