using Microsoft.AspNetCore.Http;

namespace NP.Lti13Platform.DeepLinking
{
    public interface IDeepLinkContentHandler
    {
        Task<IResult> HandleAsync(DeepLinkResponse response);
    }
}
