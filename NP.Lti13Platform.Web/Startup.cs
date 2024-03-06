using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NP.Lti13Platform.Core;

namespace NP.Lti13Platform.Web
{
    public static class Startup
    {
        public static IServiceCollection AddLti13PlatformWeb(this IServiceCollection services, Func<Lti13PlatformWebConfig, Lti13PlatformWebConfig>? configure = null)
        {
            services = services.AddLti13PlatformCore(c =>
            {
                var config = new Lti13PlatformWebConfig();

                c.CopyTo(config);

                if (configure != null)
                {
                    config = configure.Invoke(config);
                }

                services.AddSingleton(config);

                return config;
            });

            services.AddTransient<AuthenticationHandler>();

            return services;
        }

        public static IEndpointRouteBuilder UseLti13PlatformWeb(this IEndpointRouteBuilder app, Action<Lti13PlatformEndpointsConfig>? configure = null)
        {
            var config = new Lti13PlatformEndpointsConfig();

            configure?.Invoke(config);

            app.MapGet(config.AuthorizationUrl, async ([AsParameters] AuthenticationRequest qs, AuthenticationHandler handler) => await handler.HandleAsync(qs));
            app.MapPost(config.AuthorizationUrl, async ([FromForm] AuthenticationRequest body, AuthenticationHandler handler) => await handler.HandleAsync(body)).DisableAntiforgery();

            return app;
        }
    }
}
