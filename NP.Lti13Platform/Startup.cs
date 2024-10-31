using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NP.Lti13Platform.AssignmentGradeServices;
using NP.Lti13Platform.AssignmentGradeServices.Configs;
using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.Core.Services;
using NP.Lti13Platform.DeepLinking;
using NP.Lti13Platform.DeepLinking.Configs;
using NP.Lti13Platform.DeepLinking.Services;
using NP.Lti13Platform.NameRoleProvisioningServices;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;
using NP.Lti13Platform.NameRoleProvisioningServices.Services;

namespace NP.Lti13Platform
{
    public static class Startup
    {
        public static Lti13PlatformBuilder AddLti13Platform(this IServiceCollection services)
        {
            return services
                .AddLti13PlatformCore()
                .AddLti13PlatformDeepLinking()
                .AddLti13PlatformNameRoleProvisioningServices()
                .AddLti13PlatformAssignmentGradeServices();
        }

        public static Lti13PlatformBuilder WithLti13DataService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
            where T : ILti13DataService
        {
            builder.Services.TryAdd(new ServiceDescriptor(typeof(ILti13CoreDataService), typeof(T), serviceLifetime));
            builder.Services.TryAdd(new ServiceDescriptor(typeof(ILti13DeepLinkingDataService), typeof(T), serviceLifetime));
            builder.Services.TryAdd(new ServiceDescriptor(typeof(ILti13NameRoleProvisioningDataService), typeof(T), serviceLifetime));
            builder.Services.TryAdd(new ServiceDescriptor(typeof(ILti13AssignmentGradeDataService), typeof(T), serviceLifetime));

            return builder;
        }

        public static IEndpointRouteBuilder UseLti13Platform(this IEndpointRouteBuilder app, Func<Lti13PlatformEndpointsConfig, Lti13PlatformEndpointsConfig>? configure = null)
        {
            Lti13PlatformEndpointsConfig config = new();
            config = configure?.Invoke(config) ?? config;

            return app
                .UseLti13PlatformCore(x => config.Core ?? x)
                .UseLti13PlatformDeepLinking(x => config.DeepLinking ?? x)
                .UseLti13PlatformNameRoleProvisioningServices(x => config.NameRoleProvisioningServices ?? x)
                .UseLti13PlatformAssignmentGradeServices(x => config.AssignmentGradeServices ?? x);
        }
    }

    public class Lti13PlatformEndpointsConfig
    {
        public Lti13PlatformCoreEndpointsConfig? Core { get; set; }
        public DeepLinkingEndpointsConfig? DeepLinking { get; set; }
        public EndpointsConfig? NameRoleProvisioningServices { get; set; }
        public ServiceEndpointsConfig? AssignmentGradeServices { get; set; }
    }
}