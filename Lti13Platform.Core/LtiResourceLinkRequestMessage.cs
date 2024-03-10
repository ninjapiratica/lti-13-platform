
namespace NP.Lti13Platform
{
    public class LtiResourceLinkRequestMessage : ILti13Message
    {
        public required string Target_Link_Uri { get; set; }
        public required string Resource_Link_Id { get; set; }

        public string? Resource_Link_Description { get; set; }
        public string? Resource_Link_Title { get; set; }

        public IDictionary<string, object> GetClaims()
        {
            var dict = new Dictionary<string, object>
            {
                { "id", Resource_Link_Id },
            };

            if (!string.IsNullOrWhiteSpace(Resource_Link_Description)) dict.Add("description", Resource_Link_Description);
            if (!string.IsNullOrWhiteSpace(Resource_Link_Title)) dict.Add("title", Resource_Link_Title);

            return new Dictionary<string, object>
            {
                {"https://purl.imsglobal.org/spec/lti/claim/target_link_uri", Target_Link_Uri },
                {"https://purl.imsglobal.org/spec/lti/claim/resource_link", dict }
            };

            // TODO: LIS
            //new Claim("https://purl.imsglobal.org/spec/lti/claim/lis", "") // https://www.imsglobal.org/spec/lti/v1p3/#learning-information-services-lis-claim
        }
    }
}
