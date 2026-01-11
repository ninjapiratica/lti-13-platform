using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.MessageHandlers;

/// <summary>
/// Non-generic base interface for LTI resource link message extensions.
/// </summary>
/// <remarks>This interface exists to allow dependency injection to collect all extension implementations regardless of their generic parameter.
/// Implementations should inherit from ILtiResourceLinkMessageExtension&lt;TMessage&gt; where TMessage is an interface extending ILtiResourceLinkRequestMessage.</remarks>
public interface ILtiResourceLinkMessageExtension
{
}

/// <summary>
/// Defines a contract for providing resource link message extensions for a specific tool and resource link within an LTI integration.
/// </summary>
/// <remarks>Implementations of this interface allow extensions to modify a message object that implements the specified interface TMessage.
/// The extension receives a message instance and can populate or modify its properties to extend the base LTI message.</remarks>
/// <typeparam name="TMessage">The interface type representing the message that can be extended. Must implement ILtiResourceLinkRequestMessage.</typeparam>
public interface ILtiResourceLinkMessageExtension<TMessage> : ILtiResourceLinkMessageExtension
    where TMessage : ILtiResourceLinkRequestMessage
{
    /// <summary>
    /// Asynchronously extends the specified message with additional properties for the given tool and resource link.
    /// </summary>
    /// <param name="message">The message instance to be extended. Cannot be null. The implementation should populate or modify properties on this instance.</param>
    /// <param name="tool">The tool for which to provide message extensions. Cannot be null.</param>
    /// <param name="resourceLink">The resource link associated with the message extension. Cannot be null.</param>
    /// <param name="user">The user for whom the message extension is being retrieved. May be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExtendMessageAsync(TMessage message, Tool tool, ResourceLink resourceLink, User? user, CancellationToken cancellationToken = default);
}