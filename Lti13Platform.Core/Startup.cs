using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace NP.Lti13Platform.Core
{
    public static class Startup
    {
        public static IServiceCollection AddLti13Platform(this IServiceCollection services)
        {
            return services;
        }

        public static IEndpointRouteBuilder UseLti13Platform(this IEndpointRouteBuilder app, Action<Lti13PlatformConfig>? config = null)
        {
            var obj = new Lti13PlatformConfig();

            config?.Invoke(obj);

            app.MapGet(obj.AuthorizationUrl, ([FromQuery] object qs) =>
            {
                return "";
            });

            app.MapPost(obj.AuthorizationUrl, ([FromForm] object body) =>
            {
                return "";
            });

            app.Use(async (context, next) =>
            {
                await next();
            });

            return app;
        }
    }
}
