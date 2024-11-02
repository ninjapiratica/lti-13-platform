using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.DeepLinking.Models;

namespace NP.Lti13Platform.DeepLinking.Services
{
    public interface ILti13DeepLinkingDataService
    {
        Task<string> SaveContentItemAsync(string deploymentId, string? contextId, ContentItem contentItem, CancellationToken cancellationToken = default);

        Task<string> SaveLineItemAsync(LineItem lineItem, CancellationToken cancellationToken = default);
    }
}
