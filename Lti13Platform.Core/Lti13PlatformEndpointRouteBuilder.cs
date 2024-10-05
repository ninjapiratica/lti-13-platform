using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace NP.Lti13Platform.Core
{
    public class Lti13PlatformEndpointRouteBuilder(IEndpointRouteBuilder builder) : IEndpointRouteBuilder
    {
        public IServiceProvider ServiceProvider => builder.ServiceProvider;

        public ICollection<EndpointDataSource> DataSources => builder.DataSources;

        public IApplicationBuilder CreateApplicationBuilder() => builder.CreateApplicationBuilder();
    }
}
