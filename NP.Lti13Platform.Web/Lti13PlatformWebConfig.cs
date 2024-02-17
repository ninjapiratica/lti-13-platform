using NP.Lti13Platform.Core;

namespace NP.Lti13Platform.Web
{
    public class Lti13PlatformWebConfig
    {
        public int IdTokenExpirationMinutes { get; set; } = 5;

        public required Lti13PlatformCoreConfig Core { get; set; }
    }
}