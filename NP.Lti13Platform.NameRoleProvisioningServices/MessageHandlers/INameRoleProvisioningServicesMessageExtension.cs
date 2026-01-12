using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.NameRoleProvisioningServices.MessageHandlers;

/// <summary>
/// Non-generic base interface for Name and Role Provisioning Services message extensions.
/// </summary>
/// <remarks>This interface provides a non-generic entry point for invoking message extensions without reflection.
/// Implementations should also inherit from INameRoleProvisioningServicesMessageExtension&lt;TMessage&gt; for strongly-typed access.</remarks>
public interface INameRoleProvisioningServicesMessageExtension
{
    /// <summary>
    /// Asynchronously extends the specified messages with additional properties for the given tool and resource link.
    /// </summary>
    /// <remarks>This method accepts the messages dictionary with objects. Implementations must cast to the appropriate type.</remarks>
    /// <param name="messages">The message instances to be extended, indexed by user ID. Cannot be null.</param>
    /// <param name="tool">The tool for which to provide message extensions. Cannot be null.</param>
    /// <param name="resourceLink">The resource link associated with the message extension. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExtendMessagesAsync(IDictionary<UserId, object> messages, Tool tool, ResourceLink resourceLink, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a contract for extending name and role provisioning message objects associated with a specific tool and resource link in the context of LTI Name and Role Provisioning Services.
/// </summary>
/// <remarks>Implementations of this interface allow extensions to modify message objects.
/// The extension receives message instances per user (indexed by user ID) and can populate or modify their properties to extend the base LTI message.
/// The extension does not need to know about the full message contract, only the properties it needs to extend.</remarks>
/// <typeparam name="TMessage">The type representing the message that can be extended.</typeparam>
public interface INameRoleProvisioningServicesMessageExtension<TMessage> : INameRoleProvisioningServicesMessageExtension
{
    /// <summary>
    /// Asynchronously extends the specified messages with additional properties for the given tool and resource link.
    /// </summary>
    /// <param name="messages">The message instances to be extended, indexed by user ID. Cannot be null. Implementations should populate or modify properties on each message.</param>
    /// <param name="tool">The tool for which to provide message extensions. Cannot be null.</param>
    /// <param name="resourceLink">The resource link associated with the message extension. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExtendMessagesAsync(IDictionary<UserId, TMessage> messages, Tool tool, ResourceLink resourceLink, CancellationToken cancellationToken = default);

    // Explicit implementation of the non-generic interface method
    async Task INameRoleProvisioningServicesMessageExtension.ExtendMessagesAsync(IDictionary<UserId, object> messages, Tool tool, ResourceLink resourceLink, CancellationToken cancellationToken)
    {
        // Cast the dictionary to the typed version
        var typedMessages = new Dictionary<UserId, TMessage>();
        foreach (var kvp in messages)
        {
            if (kvp.Value is not TMessage typedMessage)
            {
                throw new InvalidCastException($"Message must be of type {typeof(TMessage).Name}, but received {kvp.Value.GetType().Name}");
            }
            typedMessages[kvp.Key] = typedMessage;
        }

        await ExtendMessagesAsync(typedMessages, tool, resourceLink, cancellationToken);
    }
}