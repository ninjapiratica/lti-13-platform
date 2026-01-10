using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.NameRoleProvisioningServices.MessageHandlers;

/// <summary>
/// Defines a contract for asynchronously retrieving a message extension object associated with a specific tool, resource link, and user in the context of LTI Name and Role Provisioning Services.
/// </summary>
public interface ILti13NameRoleProvisioningServicesMessageExtension
{
    /// <summary>
    /// Asynchronously retrieves a message extension object for the specified tool, resource link, and user.
    /// </summary>
    /// <param name="tool">The tool for which to retrieve the message extension. Cannot be null.</param>
    /// <param name="resourceLink">The resource link associated with the message extension. Cannot be null.</param>
    /// <param name="users">The users for whom the message extension is being retrieved. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains the message extension objects associated with the specified tool, resource link, and users.
    /// The order of the result should be the same as the order of the users provided with NULL in place of those that don't have a result.</returns>
    Task<IEnumerable<object>> GetMessageExtensionAsync(Tool tool, ResourceLink resourceLink, IEnumerable<User> users, CancellationToken cancellationToken = default);
}