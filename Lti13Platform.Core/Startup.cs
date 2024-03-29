﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace NP.Lti13Platform
{
    public static class Startup
    {
        public static IServiceCollection AddLti13Platform(this IServiceCollection services, Action<Lti13PlatformConfig> configure)
        {
            services.Configure(configure);

            services.AddTransient<Service>();
            services.AddTransient<AuthenticationHandler>();
            services.AddTransient<DeepLinkHandler>();
            services.AddTransient<JwksHandler>();

            return services;
        }

        public static IEndpointRouteBuilder UseLti13Platform(this IEndpointRouteBuilder app, Action<Lti13PlatformEndpointsConfig>? configure = null)
        {
            var config = new Lti13PlatformEndpointsConfig();
            configure?.Invoke(config);

            app.MapGet(config.JwksUrl, async (JwksHandler handler) => await handler.HandleAsync());

            app.MapGet(config.AuthorizationUrl, async ([AsParameters] AuthenticationRequest qs, AuthenticationHandler handler) => await handler.HandleAsync(qs));
            app.MapPost(config.AuthorizationUrl, async ([FromForm] AuthenticationRequest body, AuthenticationHandler handler) => await handler.HandleAsync(body)).DisableAntiforgery();

            app.MapPost(config.DeepLinkResponseUrl, async ([FromForm] DeepLinkResponseRequest body, DeepLinkHandler handler) => await handler.HandleAsync(body)).DisableAntiforgery();

            return app;
        }
    }
}
