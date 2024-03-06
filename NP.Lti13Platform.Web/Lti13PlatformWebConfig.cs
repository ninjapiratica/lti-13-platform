using NP.Lti13Platform.Core;

namespace NP.Lti13Platform.Web
{
    public class Lti13PlatformWebConfig : Lti13PlatformCoreConfig
    {
        public int IdTokenExpirationMinutes { get; set; } = 5;

        public Lti13PlatformClaim? PlatformClaim { get; set; }
    }
}