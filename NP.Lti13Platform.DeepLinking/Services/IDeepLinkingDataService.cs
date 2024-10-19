using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.DeepLinking.Models;

namespace NP.Lti13Platform.DeepLinking.Services
{
    public interface IDeepLinkingDataService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deploymentId"></param>
        /// <param name="contextId"></param>
        /// <param name="contentItem"></param>
        /// <returns>The id of the content item.</returns>
        Task<string> SaveContentItemAsync(string deploymentId, string? contextId, ContentItem contentItem, CancellationToken cancellationToken = default);

        Task<string> SaveLineItemAsync(LineItem lineItem, CancellationToken cancellationToken = default);
    }
}
