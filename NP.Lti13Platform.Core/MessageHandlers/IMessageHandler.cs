using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.MessageHandlers;

/// <summary>
/// Defines a contract for handling Learning Tools Interoperability (LTI) messages asynchronously and returning the result of the message processing operation.
/// </summary>
public interface IMessageHandler
{
    /// <summary>
    /// Processes an LTI (Learning Tools Interoperability) message asynchronously and returns the result of the message handling operation.
    /// </summary>
    /// <param name="loginHint">A unique identifier provided by the platform to correlate the login request with the user. Cannot be null or empty.</param>
    /// <param name="ltiMessageHint">An optional hint provided by the platform to help identify the specific LTI message or context. May be null.</param>
    /// <param name="tool">The tool configuration used to validate and process the LTI message. Cannot be null.</param>
    /// <param name="nonce">A unique, random string used to prevent replay attacks. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an MessageResult describing the outcome of the message handling.</returns>
    Task<MessageResult> HandleMessageAsync(string loginHint, string? ltiMessageHint, Tool tool, string nonce, CancellationToken cancellationToken = default);
}