using NP.Lti13Platform.AssignmentGradeServices.Configs;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.AssignmentGradeServices.Services;

/// <summary>
/// Defines the contract for a service that provides assignment and grade configuration for LTI 1.3 integrations.
/// </summary>
public interface ILti13AssignmentGradeConfigService
{
    /// <summary>
    /// Gets the assignment and grade services configuration for a specific client.
    /// </summary>
    /// <param name="clientId">The tool identifier for which to retrieve configuration.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the services configuration.</returns>
    Task<ServicesConfig> GetConfigAsync(ClientId clientId, CancellationToken cancellationToken = default);
}