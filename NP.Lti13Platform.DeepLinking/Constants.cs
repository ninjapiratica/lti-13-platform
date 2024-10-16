namespace NP.Lti13Platform.DeepLinking
{
    internal static class RouteNames
    {
        internal const string DEEP_LINKING_RESPONSE = "DEEP_LINKING_RESPONSE";
    }

    public static class Lti13MessageType
    {
        public const string LtiDeepLinkingRequest = "LtiDeepLinkingRequest";
    }

    /// <summary>
    /// Used for DeepLinking accept_types
    /// </summary>
    public static class Lti13DeepLinkingTypes
    {
        public const string Link = "link";
        public const string File = "file";
        public const string Html = "html";
        public const string LtiResourceLink = "ltiResourceLink";
        public const string Image = "image";
    }
}
