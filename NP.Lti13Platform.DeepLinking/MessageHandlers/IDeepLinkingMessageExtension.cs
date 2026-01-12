using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.DeepLinking.MessageHandlers;

/// <summary>
/// Non-generic base interface for deep linking message extensions.
/// </summary>
/// <remarks>This interface provides a non-generic entry point for invoking message extensions without reflection.
/// Implementations should also inherit from IDeepLinkingMessageExtension&lt;TMessage&gt; for strongly-typed access.</remarks>
public interface IDeepLinkingMessageExtension
{
    /// <summary>
    /// Asynchronously extends the specified message with additional properties for the given tool and deployment.
    /// </summary>
    /// <remarks>This method accepts the message as an object. Implementations must cast to the appropriate type.</remarks>
    /// <param name="message">The message instance to be extended. Cannot be null.</param>
    /// <param name="tool">The tool for which to provide message extensions. Cannot be null.</param>
    /// <param name="deployment">The deployment associated with the message extension. Cannot be null.</param>
    /// <param name="context">The context in which the message extension is being applied. May be null.</param>
    /// <param name="user">The user for whom the message extension is being retrieved. May be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExtendMessageAsync(object message, Tool tool, Deployment deployment, Context? context, User? user, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a contract for extending deep linking message objects associated with a specific tool and deployment within an LTI integration.
/// </summary>
/// <remarks>Implementations of this interface allow extensions to modify a message object.
/// The extension receives a message instance and can populate or modify its properties to extend the base LTI deep linking message.
/// The extension does not need to know about the full message contract, only the properties it needs to extend.</remarks>
/// <typeparam name="TMessage">The type representing the message that can be extended.</typeparam>
public interface IDeepLinkingMessageExtension<TMessage> : IDeepLinkingMessageExtension
{
    /// <summary>
    /// Asynchronously extends the specified message with additional properties for the given tool and deployment.
    /// </summary>
    /// <param name="message">The message instance to be extended. Cannot be null. The implementation should populate or modify properties on this instance.</param>
    /// <param name="tool">The tool for which to provide message extensions. Cannot be null.</param>
    /// <param name="deployment">The deployment associated with the message extension. Cannot be null.</param>
    /// <param name="context">The context in which the message extension is being applied. May be null.</param>
    /// <param name="user">The user for whom the message extension is being retrieved. May be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExtendMessageAsync(TMessage message, Tool tool, Deployment deployment, Context? context, User? user, CancellationToken cancellationToken = default);

    // Explicit implementation of the non-generic interface method
    async Task IDeepLinkingMessageExtension.ExtendMessageAsync(object message, Tool tool, Deployment deployment, Context? context, User? user, CancellationToken cancellationToken)
    {
        if (message is not TMessage typedMessage)
        {
            throw new InvalidCastException($"Message must be of type {typeof(TMessage).Name}, but received {message.GetType().Name}");
        }

        await ExtendMessageAsync(typedMessage, tool, deployment, context, user, cancellationToken);
    }
}