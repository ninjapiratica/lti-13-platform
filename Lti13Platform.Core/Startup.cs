using Microsoft.Extensions.DependencyInjection;

namespace NP.Lti13Platform.Core
{
    public static class Startup
    {
        public static IServiceCollection AddLti13PlatformCore(this IServiceCollection services, Action<Lti13PlatformCoreConfig>? configure = null)
        {
            var config = new Lti13PlatformCoreConfig();
            configure?.Invoke(config);

            services.AddSingleton(config);

            services.AddTransient<LtiResourceLinkRequestMessage>();
            services.AddTransient<LtiDeepLinkingRequestMessage>();

            services.AddTransient<UserIdentity>();

            return services;
        }
    }
}
