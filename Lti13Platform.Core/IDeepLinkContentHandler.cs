using Microsoft.AspNetCore.Http;

namespace NP.Lti13Platform
{
    public interface IDeepLinkContentHandler
    {
        Task<IResult> HandleAsync(DeepLinkResponse response);
    }
}
