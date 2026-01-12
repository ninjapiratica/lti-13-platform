using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.MessageHandlers;

/// <summary>
/// Non-generic base interface for LTI resource link message extensions.
/// </summary>
/// <remarks>This interface exists to allow dependency injection to collect all extension implementations regardless of their generic parameter.
/// Implementations should inherit from ILtiResourceLinkMessageExtension&lt;TMessage&gt; where TMessage is an interface extending ILtiResourceLinkRequestMessage.</remarks>
public interface ILtiResourceLinkMessageExtension
{
    /// <summary>
    /// Asynchronously extends the specified message with additional information using the provided tool and resource link, optionally in the context of a user.
    /// </summary>
    /// <param name="message">The message object to be extended. Must not be null.</param>
    /// <param name="tool">The tool used to process and extend the message. Must not be null.</param>
    /// <param name="resourceLink">A link to the resource that provides additional information for extending the message. Must not be null.</param>
    /// <param name="user">The user context for the operation. If null, the extension is performed without user-specific context.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation is canceled if the token is triggered.</param>
    /// <returns>A task that represents the asynchronous operation of extending the message.</returns>
    Task ExtendMessageAsync(object message, Tool tool, ResourceLink resourceLink, User? user, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a contract for providing resource link message extensions for a specific tool and resource link within an LTI integration.
/// </summary>
/// <remarks>Implementations of this interface allow extensions to modify a message object.
/// The extension receives a message instance and can populate or modify its properties to extend the base LTI message.
/// The extension does not need to know about the full message contract, only the properties it needs to extend.</remarks>
/// <typeparam name="TMessage">The type representing the message that can be extended.</typeparam>
public interface ILtiResourceLinkMessageExtension<TMessage> : ILtiResourceLinkMessageExtension
{
    /// <summary>
    /// Extends the specified message with additional information using the provided tool and resource link, optionally associating the operation with a user.
    /// </summary>
    /// <param name="message">The message to be extended. Cannot be null.</param>
    /// <param name="tool">The tool used to perform the extension. Cannot be null.</param>
    /// <param name="resourceLink">The resource link that provides context or data for the extension. Cannot be null.</param>
    /// <param name="user">The user associated with the extension operation, or null if no user context is required.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation of extending the message.</returns>
    Task ExtendMessageAsync(TMessage message, Tool tool, ResourceLink resourceLink, User? user, CancellationToken cancellationToken = default);

    // Strongly-typed implementation
    Task ILtiResourceLinkMessageExtension.ExtendMessageAsync(object message, Tool tool, ResourceLink resourceLink, User? user, CancellationToken cancellationToken)
    {
        if (message is not TMessage typedMessage)
        {
            throw new InvalidCastException($"Message must be of type {typeof(TMessage).Name}");
        }

        return ExtendMessageAsync(typedMessage, tool, resourceLink, user, cancellationToken);
    }
}