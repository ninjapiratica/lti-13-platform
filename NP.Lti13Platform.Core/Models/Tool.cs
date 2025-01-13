namespace NP.Lti13Platform.Core.Models;

public class Tool
{
    public required string Id { get; set; }

    public required string ClientId { get; set; }

    public required Uri OidcInitiationUrl { get; set; }

    public required Uri DeepLinkUrl { get; set; }

    public required Uri LaunchUrl { get; set; }

    public IEnumerable<Uri> RedirectUrls => [DeepLinkUrl, LaunchUrl];

    public Jwks? Jwks { get; set; }

    public IDictionary<string, string>? Custom { get; set; }

    public IEnumerable<string> ServiceScopes { get; set; } = [];
}
