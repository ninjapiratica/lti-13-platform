using Microsoft.AspNetCore.Http;
using System.Net.Mime;

namespace NP.Lti13Platform.DeepLinking.Services
{
    internal class DefaultDeepLinkingHandler() : ILti13DeepLinkingHandler
    {
        public Task<IResult> HandleResponseAsync(string clientId, string deploymentId, string? contextId, DeepLinkResponse response, CancellationToken cancellationToken = default) =>
            Task.FromResult(Results.Content(@$"<!DOCTYPE html>
                <html>
                <body>
                <p>This is the end of the Deep Linking flow. Please override the {nameof(ILti13DeepLinkingHandler)} for a better experience.</p>
                </body>
                </html>",
                MediaTypeNames.Text.Html));
    }
}
