using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.DeepLinking.MessageHandlers;

/// <summary>
/// Defines a contract for providing deep linking message extensions for a specific tool and deployment within an LTI integration.
/// </summary>
/// <remarks>Implementations of this interface enable customization or extension of LTI deep linking messages based on the tool, deployment, context, or user.
/// This is typically used to add additional claims or data to the deep linking response in LTI 1.3 workflows.</remarks>
public interface IDeepLinkingMessageExtension
{
    /// <summary>
    /// Asynchronously retrieves a message extension object for the specified tool and deployment, using the provided context and user information.
    /// </summary>
    /// <param name="tool">The tool for which to retrieve the message extension. Cannot be null.</param>
    /// <param name="deployment">The deployment associated with the tool. Cannot be null.</param>
    /// <param name="context">The context in which the message extension is requested. May be null if no specific context is required.</param>
    /// <param name="user">The user for whom the message extension is being retrieved. May be null if user information is not applicable.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the message extension object, or null if no extension is available.</returns>
    Task<object> GetMessageExtensionAsync(Tool tool, Deployment deployment, Context? context, User? user, CancellationToken cancellationToken = default);
}