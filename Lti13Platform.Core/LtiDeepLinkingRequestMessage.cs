using System.Text.Json;

namespace NP.Lti13Platform.Core
{
    public class LtiDeepLinkingRequestMessage : ILti13Message
    {
        public required string Deep_Link_Return_Url { get; set; }
        public required IEnumerable<string> Accept_Types { get; set; }
        public required IEnumerable<string> Accept_Presentation_Document_Targets { get; set; }

        public IEnumerable<string> Accept_Media_Types { get; set; } = [];
        public bool? Accept_Multiple { get; set; }
        public bool? Accept_LineItem { get; set; }
        public bool? Auto_Create { get; set; }
        public string? Title { get; set; }
        public string? Text { get; set; }
        public string? Data { get; set; }

        public IDictionary<string, object> GetClaims()
        {
            var dict = new Dictionary<string, object>
            {
                { "deep_link_return_url", Deep_Link_Return_Url },
                { "accept_types", JsonSerializer.SerializeToElement(Accept_Types) },
                { "accept_presentation_document_targets", JsonSerializer.SerializeToElement(Accept_Presentation_Document_Targets) },
            };

            if (Accept_Media_Types.Any()) dict.Add("accept_media_types", JsonSerializer.SerializeToElement(Accept_Media_Types));
            if (Accept_Multiple.HasValue) dict.Add("accept_multiple", Accept_Multiple);
            if (Accept_LineItem.HasValue) dict.Add("accept_lineitem", Accept_LineItem);
            if (Auto_Create.HasValue) dict.Add("auto_create", Auto_Create);
            if (!string.IsNullOrWhiteSpace(Title)) dict.Add("title", Title);
            if (!string.IsNullOrWhiteSpace(Text)) dict.Add("text", Text);
            if (!string.IsNullOrWhiteSpace(Data)) dict.Add("data", Data);

            return new Dictionary<string, object> { { "https://purl.imsglobal.org/spec/lti-dl/claim/deep_linking_settings", dict } };
        }
    }
}
