using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.Core.Models;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.MessageClaims;

/// <summary>
/// Defines the base contract for all LTI (Learning Tools Interoperability) 1.3 message types.
/// </summary>
/// <remarks>Implement this interface to represent a message that conforms to the LTI specification.
/// This interface serves as a marker for LTI message types and may be extended by more specific LTI message interfaces or classes.</remarks>
public interface IBaseLti13Message
{

}

/// <summary>
/// Represents an LTI message that can be sent between a platform and tool.
/// This follows the JWT format as defined in the LTI 1.3 Core specification and includes
/// standard OpenID Connect claims along with LTI-specific claims.
/// </summary>
public interface ILti13Message : IBaseLti13Message
{
    /// <summary>
    /// Gets or sets the issuer of the message.
    /// Issuer identifier of the platform instance initiating the launch.
    /// Required for all messages.
    /// </summary>
    [JsonPropertyName("iss")]
    public string Issuer { get; set; }

    /// <summary>
    /// Gets or sets the audience of the message.
    /// OAuth 2.0 Client ID of the tool deployment that is the audience for this message.
    /// Required for all messages.
    /// </summary>
    [JsonPropertyName("aud")]
    public string Audience { get; set; }

    /// <summary>
    /// Gets the expiration date as a Unix timestamp.
    /// Time at which the JWT MUST NOT be accepted for processing.
    /// Required for all messages.
    /// </summary>
    [JsonPropertyName("exp")]
    public long ExpirationDateUnix => new DateTimeOffset(ExpirationDate).ToUnixTimeSeconds();

    /// <summary>
    /// Gets or sets the expiration date of the message.
    /// </summary>
    [JsonIgnore]
    public DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Gets the issued date as a Unix timestamp.
    /// Time at which the JWT was issued.
    /// Required for all messages.
    /// </summary>
    [JsonPropertyName("iat")]
    public long IssuedDateUnix => new DateTimeOffset(IssuedDate).ToUnixTimeSeconds();

    /// <summary>
    /// Gets or sets the issued date of the message.
    /// </summary>
    [JsonIgnore]
    public DateTime IssuedDate { get; set; }

    /// <summary>
    /// Gets or sets the nonce of the message.
    /// String value used to associate a Client session with an ID Token and to mitigate replay attacks.
    /// This is a unique value for each launch from a given issuer.
    /// Required for all messages.
    /// </summary>
    [JsonPropertyName("nonce")]
    public string Nonce { get; set; }

    /// <summary>
    /// Gets or sets the message type.
    /// String indicating what type of LTI message is being sent.
    /// Required for all messages.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/message_type")]
    public string MessageType { get; set; }
}

/// <summary>
/// Provides extension methods for populating LTI message objects with standard LTI 1.3 claims and fields.
/// </summary>
/// <remarks>This class contains static extension methods that assist in setting required properties on LTI
/// message objects, such as audience, expiration, issue date, issuer, message type, and nonce, according to the LTI 1.3
/// specification. These methods are intended to simplify the process of preparing LTI messages for use in
/// authentication and authorization scenarios.</remarks>
public static partial class ClaimsExtensions
{
    /// <summary>
    /// Populates the specified LTI message object with standard LTI 1.3 fields, including audience, expiration, issue date, issuer, message type, and nonce.
    /// </summary>
    /// <remarks>This method sets the audience, expiration date, issued date, issuer, message type, and nonce
    /// fields on the LTI message object. The expiration date is calculated based on the current UTC time and the token
    /// expiration setting from <paramref name="tokenConfig"/>.</remarks>
    /// <typeparam name="T">The type of LTI message to populate. Must implement <see cref="ILti13Message"/>.</typeparam>
    /// <param name="obj">The LTI message object to be populated with standard fields. Must not be null.</param>
    /// <param name="messageType">The LTI message type to assign to the message. Cannot be null or empty.</param>
    /// <param name="nonce">A unique value used to prevent replay attacks. Cannot be null or empty.</param>
    /// <param name="clientId">The client identifier representing the audience for the message. Cannot be null.</param>
    /// <param name="tokenConfig">The platform token configuration containing issuer information and token expiration settings. Cannot be null.</param>
    /// <returns>The same LTI message object with its standard fields populated according to the provided parameters.</returns>
    public static T WithLti13MessageClaims<T>(
        this T obj,
        string messageType,
        string nonce,
        ClientId clientId,
        TokenConfig tokenConfig)
        where T : ILti13Message
    {
        obj.Audience = clientId.ToString();
        obj.ExpirationDate = DateTime.UtcNow.AddSeconds(tokenConfig.MessageTokenExpirationSeconds);
        obj.IssuedDate = DateTime.UtcNow;
        obj.Issuer = tokenConfig.Issuer.OriginalString;
        obj.MessageType = messageType;
        obj.Nonce = nonce;

        return obj;
    }
}
