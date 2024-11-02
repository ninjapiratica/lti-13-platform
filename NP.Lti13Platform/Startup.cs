using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NP.Lti13Platform.AssignmentGradeServices;
using NP.Lti13Platform.AssignmentGradeServices.Configs;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.DeepLinking;
using NP.Lti13Platform.DeepLinking.Configs;
using NP.Lti13Platform.NameRoleProvisioningServices;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;

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
            builder.WithLti13CoreDataService<T>(serviceLifetime)
                .WithLti13DeepLinkingDataService<T>(serviceLifetime)
                .WithLti13NameRoleProvisioningDataService<T>(serviceLifetime)
                .WithLti13AssignmentGradeDataService<T>(serviceLifetime);

            return builder;
        }

        public static IEndpointRouteBuilder UseLti13Platform(this IEndpointRouteBuilder app, Func<Lti13PlatformEndpointsConfig, Lti13PlatformEndpointsConfig>? configure = null)
        {
            Lti13PlatformEndpointsConfig config = new();
            config = configure?.Invoke(config) ?? config;

            return app
                .UseLti13PlatformCore(x => config.Core)
                .UseLti13PlatformDeepLinking(x => config.DeepLinking)
                .UseLti13PlatformNameRoleProvisioningServices(x => config.NameRoleProvisioningServices)
                .UseLti13PlatformAssignmentGradeServices(x => config.AssignmentGradeServices);
        }
    }

    public class Lti13PlatformEndpointsConfig
    {
        public Lti13PlatformCoreEndpointsConfig Core { get; set; } = new();
        public DeepLinkingEndpointsConfig DeepLinking { get; set; } = new();
        public EndpointsConfig NameRoleProvisioningServices { get; set; } = new();
        public ServiceEndpointsConfig AssignmentGradeServices { get; set; } = new();
    }
}