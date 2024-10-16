using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace NP.Lti13Platform.Core.Services
{
    public class LtiLinkGenerator(LinkGenerator linkGenerator)
    {
        public string GetUriString(string endpointName, object endpointValues, HttpRequest httpRequest, Uri? baseUri = null)
        {
            var scheme = httpRequest.Scheme;
            var host = httpRequest.Host;

            if (baseUri != null)
            {
                scheme = baseUri.Scheme;
                host = HostString.FromUriComponent(baseUri);
            }

            return linkGenerator.GetUriByName(endpointName, endpointValues, scheme, host) ?? string.Empty;
        }
    }
}
