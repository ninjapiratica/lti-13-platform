namespace NP.Lti13Platform.Models
{
    public class Client
    {
        public required Guid Id { get; set; }

        public required string OidcInitiationUrl { get; set; }

        public required string DeepLinkUrl { get; set; }

        public required string LaunchUrl { get; set; }

        public IEnumerable<string> RedirectUrls => new[] { DeepLinkUrl, LaunchUrl }.Where(x => x != null).Select(x => x!);

        public Jwks? Jwks { get; set; }

        public IEnumerable<string> Scopes { get; set; } = [];
    }
}
