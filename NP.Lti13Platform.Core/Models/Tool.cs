﻿namespace NP.Lti13Platform.Core.Models;

public class Tool
{
    public required string Id { get; set; }

    public required string ClientId { get; set; }

    public required string OidcInitiationUrl { get; set; }

    public required string DeepLinkUrl { get; set; }

    public required string LaunchUrl { get; set; }

    public IEnumerable<string> RedirectUrls => new[] { DeepLinkUrl, LaunchUrl }.Where(x => x != null).Select(x => x!);

    public Jwks? Jwks { get; set; }

    public IDictionary<string, string>? Custom { get; set; }

    public IEnumerable<string> ServiceScopes { get; set; } = [];
}
