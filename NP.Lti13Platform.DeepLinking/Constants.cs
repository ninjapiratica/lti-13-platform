namespace NP.Lti13Platform.DeepLinking
{
    internal static class RouteNames
    {
        internal static readonly string DEEP_LINKING_RESPONSE = "DEEP_LINKING_RESPONSE";
    }

    public static class Lti13MessageType
    {
        public static readonly string LtiDeepLinkingRequest = "LtiDeepLinkingRequest";
    }

    /// <summary>
    /// Used for DeepLinking accept_types
    /// </summary>
    public static class Lti13DeepLinkingTypes
    {
        public static readonly string Link = "link";
        public static readonly string File = "file";
        public static readonly string Html = "html";
        public static readonly string LtiResourceLink = "ltiResourceLink";
        public static readonly string Image = "image";
    }
}
