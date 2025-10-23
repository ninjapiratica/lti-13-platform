using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Json;

namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents a JSON Web Key Set (JWKS).
/// </summary>
public abstract class Jwks
{
    /// <summary>
    /// Create an instance of JWKS using the provided key or uri.
    /// </summary>
    /// <param name="keyOrUrl">The public key or JWKS uri to use.</param>
    /// <returns>An instance of JWKS depending on the type of string provided.</returns>
    static Jwks Create(string keyOrUrl) => Uri.IsWellFormedUriString(keyOrUrl, UriKind.Absolute) ?
            new JwksUri { Url = new Uri(keyOrUrl) } :
            new JwtPublicKey { PublicKey = keyOrUrl };

    /// <summary>
    /// Gets the security keys asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of security keys.</returns>
    public abstract Task<IEnumerable<SecurityKey>> GetKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Implicitly converts a string to a <see cref="Jwks"/> instance.
    /// </summary>
    /// <param name="keyOrUrl">The public key or JWKS URL.</param>
    public static implicit operator Jwks(string keyOrUrl) => Create(keyOrUrl);
}

/// <summary>
/// Represents a JWKS with a public key.
/// </summary>
public class JwtPublicKey : Jwks
{
    /// <summary>
    /// Gets or sets the public key.
    /// </summary>
    public required string PublicKey { get; set; }

    /// <inheritdoc />
    public override Task<IEnumerable<SecurityKey>> GetKeysAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<SecurityKey>>([new JsonWebKey(PublicKey)]);
    }
}

/// <summary>
/// Represents a JWKS with a URL.
/// </summary>
public class JwksUri : Jwks
{
    private static readonly HttpClient httpClient = new();

    /// <summary>
    /// Gets or sets the URL of the JWKS.
    /// </summary>
    public required Uri Url { get; set; }

    /// <inheritdoc />
    public override async Task<IEnumerable<SecurityKey>> GetKeysAsync(CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.GetAsync(Url, cancellationToken);
        var result = await httpResponse.Content.ReadFromJsonAsync<JsonWebKeySet>(cancellationToken);

        if (result != null)
        {
            return result.Keys;
        }

        return [];
    }
}
