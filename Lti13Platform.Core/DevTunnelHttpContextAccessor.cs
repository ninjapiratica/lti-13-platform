using Microsoft.AspNetCore.Http;

namespace NP.Lti13Platform.Core
{
    // Copied from HttpContextAccessor (with VS_TUNNEL_URL modification)
    public class DevTunnelHttpContextAccessor : IHttpContextAccessor
    {
        private static readonly AsyncLocal<HttpContextHolder> _httpContextCurrent = new();

        public HttpContext? HttpContext
        {
            get
            {
                return _httpContextCurrent.Value?.Context;
            }
            set
            {
                var holder = _httpContextCurrent.Value;
                if (holder != null)
                {
                    // Clear current HttpContext trapped in the AsyncLocals, as its done.
                    holder.Context = null;
                }

                if (value != null)
                {
                    var devTunnel = Environment.GetEnvironmentVariable("VS_TUNNEL_URL");
                    if (!string.IsNullOrWhiteSpace(devTunnel))
                    {
                        value.Request.Host = new HostString(new Uri(devTunnel).Host);
                    }

                    // Use an object indirection to hold the HttpContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _httpContextCurrent.Value = new HttpContextHolder { Context = value };
                }
            }
        }

        private sealed class HttpContextHolder
        {
            public HttpContext? Context;
        }
    }
}
