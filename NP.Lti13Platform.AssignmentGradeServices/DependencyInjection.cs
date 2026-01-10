using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NP.Lti13Platform.AssignmentGradeServices.Configs;
using NP.Lti13Platform.AssignmentGradeServices.MessageClaims;
using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core;

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
    /// Adds LTI 1.3 platform assignment grade services to the specified service collection, including configuration and required dependencies.
    /// </summary>
    /// <remarks>This method configures assignment grade services for LTI 1.3 platform integration, including binding configuration settings and registering required service implementations.
    /// It should be called during application startup to enable assignment grade functionality.</remarks>
    /// <typeparam name="T">The type that implements the assignment grade data service interface. Must implement <see cref="IAssignmentGradeDataService"/>.</typeparam>
    /// <param name="serviceCollection">The service collection to which the assignment grade services will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which the assignment grade data service will be registered. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, with assignment grade services registered.</returns>
    public static IServiceCollection AddPlatformAssignmentGradeServices<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : IAssignmentGradeDataService
    {
        serviceCollection.AddOptions<ServicesConfig>().BindConfiguration("Lti13Platform:AssignmentGradeServices");
        serviceCollection.TryAddSingleton<IAssignmentGradeConfigService, DefaultAssignmentGradeConfigService>();

        serviceCollection.Add(new ServiceDescriptor(typeof(IAssignmentGradeDataService), typeof(T), serviceLifetime));
        serviceCollection.WithResourceLinkMessageExtension<LineItemServiceMessageExtension>();

        return serviceCollection;
    }

    /// <summary>
    /// Adds an implementation of the IAssignmentGradeConfigService interface to the service collection with the specified service lifetime.
    /// </summary>
    /// <remarks>Use this method to register a custom implementation of IAssignmentGradeConfigService for dependency injection.
    /// This enables consuming components to resolve IAssignmentGradeConfigService from the service provider according to the specified lifetime.</remarks>
    /// <typeparam name="T">The type that implements IAssignmentGradeConfigService to be registered.</typeparam>
    /// <param name="serviceCollection">The IServiceCollection to which the IAssignmentGradeConfigService implementation will be added.</param>
    /// <param name="serviceLifetime">The lifetime with which the service will be registered. Defaults to ServiceLifetime.Transient.</param>
    /// <returns>The IServiceCollection instance with the IAssignmentGradeConfigService implementation registered.</returns>
    public static IServiceCollection WithAssignmentGradeConfigService<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : IAssignmentGradeConfigService
    {
        serviceCollection.Add(new ServiceDescriptor(typeof(IAssignmentGradeConfigService), typeof(T), serviceLifetime));
        return serviceCollection;
    }
}