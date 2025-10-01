using Microsoft.AspNetCore.Http;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.DeepLinking.Services;

/// <summary>
/// Defines a handler for deep linking responses in an LTI 1.3 platform.
/// </summary>
public interface ILti13DeepLinkingHandler
{
    /// <summary>
    /// Handles a deep linking response from an LTI tool.
    /// </summary>
    /// <param name="clientId">The tool identifier.</param>
    /// <param name="deploymentId">The deployment identifier.</param>
    /// <param name="contextId">Optional. The context identifier.</param>
    /// <param name="response">The deep linking response to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP result to return to the client.</returns>
    Task<IResult> HandleResponseAsync(ClientId clientId, string deploymentId, string? contextId, DeepLinkResponse response, CancellationToken cancellationToken = default);
}
