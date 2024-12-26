using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Json;

namespace NP.Lti13Platform.Core.Models;

public abstract class Jwks
{
    /// <summary>
    /// Create an instance of Jwks using the provided key or uri.
    /// </summary>
    /// <param name="keyOrUri">The public key or JWKS uri to use.</param>
    /// <returns>An instance of Jwks depending on the type of string provided.</returns>
    static Jwks Create(string keyOrUri) => Uri.IsWellFormedUriString(keyOrUri, UriKind.Absolute) ?
            new JwksUri { Uri = keyOrUri } :
            new JwtPublicKey { PublicKey = keyOrUri };

    public abstract Task<IEnumerable<SecurityKey>> GetKeysAsync(CancellationToken cancellationToken = default);

    public static implicit operator Jwks(string keyOrUri) => Create(keyOrUri);
}

public class JwtPublicKey : Jwks
{
    public required string PublicKey { get; set; }

    public override Task<IEnumerable<SecurityKey>> GetKeysAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<SecurityKey>>([new JsonWebKey(PublicKey)]);
    }
}

public class JwksUri : Jwks
{
    private static readonly HttpClient httpClient = new();

    public required string Uri { get; set; }

    public override async Task<IEnumerable<SecurityKey>> GetKeysAsync(CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.GetAsync(Uri, cancellationToken);
        var result = await httpResponse.Content.ReadFromJsonAsync<JsonWebKeySet>(cancellationToken);

        if (result != null)
        {
            return result.Keys;
        }

        return [];
    }
}
