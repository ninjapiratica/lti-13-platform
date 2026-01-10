using Microsoft.AspNetCore.Http;
using NP.Lti13Platform.Core.Models;
using System.Net.Mime;
using System.Text.Json;

namespace NP.Lti13Platform.DeepLinking.Services;

/// <summary>
/// Defines a handler for deep linking responses in an LTI 1.3 platform.
/// </summary>
public interface IDeepLinkingResponseHandler
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
    Task<IResult> HandleResponseAsync(ClientId clientId, DeploymentId deploymentId, ContextId? contextId, DeepLinkingResponse response, CancellationToken cancellationToken = default);
}

internal class DefaultDeepLinkingResponseHandler() : IDeepLinkingResponseHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public Task<IResult> HandleResponseAsync(ClientId clientId, DeploymentId deploymentId, ContextId? contextId, DeepLinkingResponse response, CancellationToken cancellationToken = default) =>
        Task.FromResult(Results.Content($@"
<!DOCTYPE html>
<html>
<body>
    <p>This is the end of the Deep Linking flow. Please override the {nameof(IDeepLinkingResponseHandler)} for a better experience.</p>
    <pre>{JsonSerializer.Serialize(response, SerializerOptions)}</pre>
</body>
</html>".TrimStart(),
            MediaTypeNames.Text.Html));
}