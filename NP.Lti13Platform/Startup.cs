﻿using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NP.Lti13Platform.AssignmentGradeServices;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.DeepLinking;
using NP.Lti13Platform.NameRoleProvisioningServices;

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

        public static Lti13PlatformBuilder AddLti13PlatformWithDefaults(
            this IServiceCollection services,
            Action<Lti13PlatformTokenConfig> configureToken,
            Action<Platform>? configurePlatform = null,
            Action<Lti13DeepLinkingConfig>? configureDeepLinking = null,
            Action<Lti13AssignmentGradeServicesConfig>? configureAssignmentGradeService = null,
            Action<Lti13NameRoleProvisioningServicesConfig>? configureNameRoleProvisioningService = null)
        {
            return services.AddLti13Platform()
                .AddDefaultTokenService(configureToken)
                .AddDefaultPlatformService(configurePlatform)
                .AddDefaultDeepLinkingService(configureDeepLinking)
                .AddDefaultAssignmentGradeService(configureAssignmentGradeService)
                .AddDefaultNameRoleProvisioningService(configureNameRoleProvisioningService);
        }

        public static Lti13PlatformBuilder AddDataService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
            where T : IDataService
        {
            builder.Services.TryAdd(new ServiceDescriptor(typeof(ICoreDataService), typeof(T), serviceLifetime));
            builder.Services.TryAdd(new ServiceDescriptor(typeof(IDeepLinkingDataService), typeof(T), serviceLifetime));
            builder.Services.TryAdd(new ServiceDescriptor(typeof(INameRoleProvisioningServicesDataService), typeof(T), serviceLifetime));
            builder.Services.TryAdd(new ServiceDescriptor(typeof(IAssignmentGradeServicesDataService), typeof(T), serviceLifetime));

            return builder;
        }

        public static Lti13PlatformEndpointRouteBuilder UseLti13Platform(this IEndpointRouteBuilder app, Action<Lti13PlatformEndpointsConfig>? configure = null)
        {
            var endpointsConfig = new Lti13PlatformEndpointsConfig();
            configure?.Invoke(endpointsConfig);

            return app
                .UseLti13PlatformCore(x =>
                {
                    if (endpointsConfig.Core != null)
                    {
                        x.JwksUrl = endpointsConfig.Core.JwksUrl;
                        x.AuthorizationUrl = endpointsConfig.Core.AuthorizationUrl;
                        x.TokenUrl = endpointsConfig.Core.TokenUrl;
                    }
                })
                .UseLti13PlatformDeepLinking(x =>
                {
                    if (endpointsConfig.DeepLinking != null)
                    {
                        x.DeepLinkingResponseUrl = endpointsConfig.DeepLinking.DeepLinkingResponseUrl;
                    }
                })
                .UseLti13PlatformNameRoleProvisioningServices(x =>
                {
                    if (endpointsConfig.NameRoleProvisioningServices != null)
                    {
                        x.NamesAndRoleProvisioningServicesUrl = endpointsConfig.NameRoleProvisioningServices.NamesAndRoleProvisioningServicesUrl;
                    }
                })
                .UseLti13PlatformAssignmentGradeServices(x =>
                {
                    if (endpointsConfig.AssignmentGradeServices != null)
                    {
                        x.LineItemUrl = endpointsConfig.AssignmentGradeServices.LineItemUrl;
                        x.LineItemsUrl = endpointsConfig.AssignmentGradeServices.LineItemsUrl;
                    }
                });
        }
    }

    public class Lti13PlatformEndpointsConfig
    {
        public Lti13PlatformCoreEndpointsConfig? Core { get; set; }
        public Lti13DeepLinkingEndpointsConfig? DeepLinking { get; set; }
        public Lti13PlatformNameRoleProvisioningServicesEndpointsConfig? NameRoleProvisioningServices { get; set; }
        public Lti13AssignmentGradeServicesEndpointsConfig? AssignmentGradeServices { get; set; }
    }
}