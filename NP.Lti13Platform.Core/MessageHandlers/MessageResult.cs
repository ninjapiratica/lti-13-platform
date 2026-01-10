namespace NP.Lti13Platform.Core.MessageHandlers;

/// <summary>
/// Represents the result of an LTI (Learning Tools Interoperability) message operation, indicating either success with
/// a message or failure with an error message.
/// </summary>
/// <remarks>This abstract base class encapsulates the outcome of processing an LTI message. Use the derived types
/// to access the specific result details. The properties indicate whether the operation succeeded and provide access to
/// the resulting message or error information as appropriate.</remarks>
public abstract class MessageResult
{
    /// <summary>
    /// Represents an LTI message result that indicates no content or outcome is returned.
    /// </summary>
    /// <remarks>Use this type when an LTI message response does not require a result payload. This is
    /// typically used to signal a successful operation where no additional data is provided.</remarks>
    public class NoneResult : MessageResult { }

    /// <summary>
    /// Represents a result indicating a successful LTI message operation.
    /// </summary>
    /// <param name="message">The LTI message object associated with the successful result. Cannot be null.</param>
    public class SuccessResult(object message) : MessageResult
    {
        /// <summary>
        /// Gets the LTI message associated with the current context.
        /// </summary>
        /// <remarks>The returned object represents the LTI (Learning Tools Interoperability) message
        /// payload. The structure and type of this object depend on the specific LTI message received. Callers should
        /// cast or process the object according to the expected LTI message type.</remarks>
        public object Message { get; } = message;
    }

    /// <summary>
    /// Represents the result of an operation that failed, including an associated error message.
    /// </summary>
    /// <param name="errorMessage">The error message that describes the reason for the failure. Cannot be null or empty.</param>
    public class ErrorResult(string errorMessage) : MessageResult
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
    public static MessageResult None()
        => new NoneResult();

    /// <summary>
    /// Creates an error result containing the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message to include in the result. Cannot be null.</param>
    /// <returns>An <see cref="MessageResult"/> representing an error, initialized with the provided message.</returns>
    public static MessageResult Error(string errorMessage)
        => new ErrorResult(errorMessage);

    /// <summary>
    /// Creates a successful LTI message result containing the specified message.
    /// </summary>
    /// <param name="ltiMessage">The LTI message to be wrapped in the success result. Cannot be null.</param>
    /// <returns>An <see cref="MessageResult"/> representing a successful outcome that contains the provided LTI message.</returns>
    public static MessageResult Success(object ltiMessage)
        => new SuccessResult(ltiMessage);
}