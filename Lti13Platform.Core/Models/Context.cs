using System.Text.Json;

namespace NP.Lti13Platform.Models
{
    public class Context : ILti13Claim
    {
        /// <summary>
        /// Max Length 255 characters
        /// Case sensitive
        /// </summary>
        public required Guid Id { get; set; }

        public required Guid DeploymentId { get; set; }

        public string? Label { get; set; }

        public string? Title { get; set; }

        public IEnumerable<string> Types { get; set; } = [];

        public IDictionary<string, object> GetClaims()
        {
            var dict = new Dictionary<string, object>();

            if (Id != null) dict.Add("id", Id);
            if (Types.Any()) dict.Add("type", JsonSerializer.SerializeToElement(Types));
            if (Label != null) dict.Add("label", Label);
            if (Title != null) dict.Add("title", Title);

            if (dict.Count > 0)
            {
                return new Dictionary<string, object>
                {
                    { "https://purl.imsglobal.org/spec/lti/claim/context", dict }
                };
            }

            return new Dictionary<string, object>();
        }
    }
}
