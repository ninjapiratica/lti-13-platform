namespace NP.Lti13Platform
{
    public class DeepLinkResponseMessage
    {
        public string? Data { get; set; }
        public string? Message { get; set; }
        public string? Log { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorLog { get; set; }
        public IEnumerable<ContentItem> ContentItems { get; set; } = [];
    }
}