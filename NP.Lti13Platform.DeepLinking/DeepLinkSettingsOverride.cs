namespace NP.Lti13Platform.DeepLinking
{
    public record DeepLinkSettingsOverride
    {
        public string? DeepLinkReturnUrl { get; set; }
        public IEnumerable<string>? AcceptTypes { get; set; }
        public IEnumerable<string>? AcceptPresentationDocumentTargets { get; set; }
        public IEnumerable<string>? AcceptMediaTypes { get; set; }
        public bool? AcceptMultiple { get; set; }
        public bool? AcceptLineItem { get; set; }
        public bool? AutoCreate { get; set; }
        public string? Title { get; set; }
        public string? Text { get; set; }
        public string? Data { get; set; }
    }
}
