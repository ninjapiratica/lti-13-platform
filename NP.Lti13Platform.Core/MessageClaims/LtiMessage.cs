using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.Core.Models;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.MessageClaims;

/// <summary>
/// Defines the base contract for all LTI (Learning Tools Interoperability) message types.
/// </summary>
/// <remarks>Implement this interface to represent a message that conforms to the LTI specification.
/// This interface serves as a marker for LTI message types and may be extended by more specific LTI message interfaces or classes.</remarks>
public interface IBaseLtiMessage
{

}

/// <summary>
/// Represents an LTI message that can be sent between a platform and tool.
/// This follows the JWT format as defined in the LTI 1.3 Core specification and includes
/// standard OpenID Connect claims along with LTI-specific claims.
/// </summary>
public interface ILtiMessage : IBaseLtiMessage
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
    /// <typeparam name="T">The type of LTI message to populate. Must implement <see cref="ILtiMessage"/>.</typeparam>
    /// <param name="obj">The LTI message object to be populated with standard fields. Must not be null.</param>
    /// <param name="messageType">The LTI message type to assign to the message. Cannot be null or empty.</param>
    /// <param name="nonce">A unique value used to prevent replay attacks. Cannot be null or empty.</param>
    /// <param name="clientId">The client identifier representing the audience for the message. Cannot be null.</param>
    /// <param name="tokenConfig">The platform token configuration containing issuer information and token expiration settings. Cannot be null.</param>
    /// <returns>The same LTI message object with its standard fields populated according to the provided parameters.</returns>
    public static T WithLtiMessageClaims<T>(
        this T obj,
        string messageType,
        string nonce,
        ClientId clientId,
        Lti13PlatformTokenConfig tokenConfig)
        where T : ILtiMessage
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


/// <summary>
/// Represents the result of an LTI (Learning Tools Interoperability) message operation, indicating either success with
/// a message or failure with an error message.
/// </summary>
/// <remarks>This abstract base class encapsulates the outcome of processing an LTI message. Use the derived types
/// to access the specific result details. The properties indicate whether the operation succeeded and provide access to
/// the resulting message or error information as appropriate.</remarks>
public abstract class LtiMessageResult
{
    /// <summary>
    /// Represents an LTI message result that indicates no content or outcome is returned.
    /// </summary>
    /// <remarks>Use this type when an LTI message response does not require a result payload. This is
    /// typically used to signal a successful operation where no additional data is provided.</remarks>
    public class NoneResult : LtiMessageResult { }

    /// <summary>
    /// Represents a result indicating a successful LTI message operation.
    /// </summary>
    /// <param name="ltiMessage">The LTI message object associated with the successful result. Cannot be null.</param>
    public class SuccessResult(object ltiMessage) : LtiMessageResult
    {
        /// <summary>
        /// Gets the LTI message associated with the current context.
        /// </summary>
        /// <remarks>The returned object represents the LTI (Learning Tools Interoperability) message
        /// payload. The structure and type of this object depend on the specific LTI message received. Callers should
        /// cast or process the object according to the expected LTI message type.</remarks>
        public object LtiMessage { get; } = ltiMessage;
    }

    /// <summary>
    /// Represents the result of an operation that failed, including an associated error message.
    /// </summary>
    /// <param name="errorMessage">The error message that describes the reason for the failure. Cannot be null or empty.</param>
    public class ErrorResult(string errorMessage) : LtiMessageResult
    {
        /// <summary>
        /// Gets the error message associated with the current operation or result.
        /// </summary>
        public string ErrorMessage { get; } = errorMessage;
    }

    /// <summary>
    /// Creates an LtiMessageResult instance that represents the absence of a result.
    /// </summary>
    /// <remarks>Use this method when an operation does not produce a result or when you need to explicitly indicate that no result is available.</remarks>
    /// <returns>An LtiMessageResult indicating that no result is present.</returns>
    public static LtiMessageResult None()
        => new NoneResult();

    /// <summary>
    /// Creates an error result containing the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message to include in the result. Cannot be null.</param>
    /// <returns>An <see cref="LtiMessageResult"/> representing an error, initialized with the provided message.</returns>
    public static LtiMessageResult Error(string errorMessage)
        => new ErrorResult(errorMessage);

    /// <summary>
    /// Creates a successful LTI message result containing the specified message.
    /// </summary>
    /// <param name="ltiMessage">The LTI message to be wrapped in the success result. Cannot be null.</param>
    /// <returns>An <see cref="LtiMessageResult"/> representing a successful outcome that contains the provided LTI message.</returns>
    public static LtiMessageResult Success(object ltiMessage)
        => new SuccessResult(ltiMessage);
}