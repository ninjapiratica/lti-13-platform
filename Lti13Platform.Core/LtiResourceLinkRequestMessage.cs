using System.Web;

namespace NP.Lti13Platform.Core
{
    public class LtiResourceLinkRequestMessage(Lti13PlatformCoreConfig config) : ILti13Message
    {
        public static string MessageType => "LtiResourceLinkRequest";

        public string Target_Link_Uri { get; set; } = string.Empty;

        public string Resource_Link_Id { get; set; } = string.Empty;
        public string? Resource_Link_Description { get; set; }
        public string? Resource_Link_Title { get; set; }

        public IEnumerable<(string Key, object Value> GetClaims()
        {
            var dict = new Dictionary<string, object>
            {
                { "id", Resource_Link_Id },
            };

            if (!string.IsNullOrWhiteSpace(Resource_Link_Description)) dict.Add("description", Resource_Link_Description);
            if (!string.IsNullOrWhiteSpace(Resource_Link_Title)) dict.Add("title", Resource_Link_Title);

            yield return ("https://purl.imsglobal.org/spec/lti/claim/target_link_uri", Target_Link_Uri);
            yield return ("https://purl.imsglobal.org/spec/lti/claim/resource_link", dict);

            // TODO: LIS
            //new Claim("https://purl.imsglobal.org/spec/lti/claim/lis", "") // https://www.imsglobal.org/spec/lti/v1p3/#learning-information-services-lis-claim
        }

        public async Task<Uri> GetUrlAsync(string clientId, string deploymentId)
        {
            var iss = config.Issuer;
            var login_hint = "";
            var target_link_uri = "";

            var lti_message_hint = "";
            var lti_deployment_id = deploymentId;
            var client_id = clientId;

            var client = await GetClientAsync(clientId);

            var uriBuilder = new UriBuilder(client.OidcInitiationUrl);
            var queryString = HttpUtility.ParseQueryString(uriBuilder.Query);

            queryString.Add("iss", iss);
            queryString.Add("login_hint", login_hint);
            queryString.Add("target_link_uri", target_link_uri);
            queryString.Add("lti_message_hint", lti_message_hint);
            queryString.Add("lti_deployment_id", lti_deployment_id);
            queryString.Add("client_id", client_id);

            uriBuilder.Query = queryString.ToString();

            return uriBuilder.Uri;
        }

        public virtual Task<Lti13Client> GetClientAsync(string clientId) => Task.FromResult<Lti13Client>(null);
    }
}
