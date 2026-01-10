using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Core.MessageHandlers;

/// <summary>
/// Defines a contract for providing resource link message extensions for a specific tool and resource link within an LTI integration.
/// </summary>
public interface ILtiResourceLinkMessageExtension
{
    /// <summary>
    /// Asynchronously retrieves a message extension object for the specified tool and resource link.
    /// </summary>
    /// <param name="tool">The tool for which to retrieve the message extension. Cannot be null.</param>
    /// <param name="resourceLink">The resource link associated with the message extension. Cannot be null.</param>
    /// <param name="user">The user for whom the message extension is being retrieved. May be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the message extension object, or null if no extension is available.</returns>
    Task<object> GetMessageExtensionAsync(Tool tool, ResourceLink resourceLink, User? user, CancellationToken cancellationToken = default);
}