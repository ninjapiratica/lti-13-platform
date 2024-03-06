using Microsoft.Extensions.DependencyInjection;

namespace NP.Lti13Platform.Core
{
    public static class Startup
    {
        public static IServiceCollection AddLti13PlatformCore(this IServiceCollection services, Func<Lti13PlatformCoreConfig, Lti13PlatformCoreConfig>? configure = null)
        {
            var config = new Lti13PlatformCoreConfig();
            if (configure != null)
            {
                config = configure.Invoke(config);
            }

            services.AddSingleton(config);
            services.AddTransient<Service>();

            return services;
        }
    }
}
