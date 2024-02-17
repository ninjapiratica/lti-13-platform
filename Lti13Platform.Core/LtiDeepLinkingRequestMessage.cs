namespace NP.Lti13Platform.Core
{
    public class LtiDeepLinkingRequestMessage : ILti13Message
    {
        public static string MessageType => "LtiDeepLinkingRequest";\

        // REQUIRED
        public string Deep_Link_Return_Url { get; set; } = string.Empty;
        public List<string> Accept_Types { get; set; } = [];
        public List<string> Accept_Presentation_Document_Targets { get; set; } = [];

        // OPTIONAL
        public IEnumerable<string> Accept_Media_Types { get; set; } = [];
        public bool? Accept_Multiple { get; set; }
        public bool? Accept_LineItem { get; set; }
        public bool? Auto_Create { get; set; }
        public string? Title { get; set; }
        public string? Text { get; set; }
        public string? Data { get; set; }

        public IEnumerable<(string Key, object Value)> GetClaims()
        {
            var dict = new Dictionary<string, object>
            {
                { "deep_link_return_url", Deep_Link_Return_Url },
                { "accept_types", Accept_Types },
                { "accept_presentation_document_targets", Accept_Presentation_Document_Targets },
            };

            if (Accept_Media_Types.Any()) dict.Add("accept_types", Accept_Types);
            if (Accept_Multiple.HasValue) dict.Add("accept_multiple", Accept_Multiple);
            if (Accept_LineItem.HasValue) dict.Add("accept_lineitem", Accept_LineItem);
            if (Auto_Create.HasValue) dict.Add("auto_create", Auto_Create);
            if (!string.IsNullOrWhiteSpace(Title)) dict.Add("title", Title);
            if (!string.IsNullOrWhiteSpace(Text)) dict.Add("text", Text);
            if (!string.IsNullOrWhiteSpace(Data)) dict.Add("data", Data);

            yield return ("https://purl.imsglobal.org/spec/lti-dl/claim/deep_linking_settings", dict);
        }

        //public async Task<Uri> GetUrlAsync(string clientId, string deploymentId)
        //{
        //    var iss = config.Issuer;
        //    var login_hint = "";
        //    var target_link_uri = "";

        //    var lti_message_hint = "";
        //    var lti_deployment_id = deploymentId;
        //    var client_id = clientId;

        //    var client = await GetClientAsync(clientId);

        //    var uriBuilder = new UriBuilder(client.OidcInitiationUrl);
        //    var queryString = HttpUtility.ParseQueryString(uriBuilder.Query);

        //    queryString.Add("iss", iss);
        //    queryString.Add("login_hint", login_hint);
        //    queryString.Add("target_link_uri", target_link_uri);
        //    queryString.Add("lti_message_hint", lti_message_hint);
        //    queryString.Add("lti_deployment_id", lti_deployment_id);
        //    queryString.Add("client_id", client_id);

        //    uriBuilder.Query = queryString.ToString();

        //    return uriBuilder.Uri;
        //}
    }
}
