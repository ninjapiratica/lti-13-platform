using NP.Lti13Platform.DeepLinking.Models;

namespace NP.Lti13Platform.DeepLinking;

public class DeepLinkResponse
{
    public string? Data { get; set; }

    public string? Message { get; set; }
    public string? Log { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorLog { get; set; }

    public IEnumerable<ContentItem> ContentItems { get; set; } = [];
}