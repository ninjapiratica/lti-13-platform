using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.Populators
{
    public interface IResourceLinkMessage
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/version")]
        public string LtiVersion { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/deployment_id")]
        public string DeploymentId { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/target_link_uri")]
        public string TargetLinkUri { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/resource_link")]
        public ResourceLinkMessage ResourceLink { get; set; }

        public class ResourceLinkMessage
        {
            [JsonPropertyName("id")]
            public required string Id { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }
        }
    }

    public class ResourceLinkPopulator() : Populator<IResourceLinkMessage>
    {
        public override async Task Populate(IResourceLinkMessage obj, Lti13MessageScope scope)
        {
            obj.LtiVersion = "1.3.0";
            obj.DeploymentId = scope.Deployment.Id;

            // TODO: use message hint
            //scope.MessageHint

            obj.TargetLinkUri = scope.ResourceLink?.Url ?? scope.Tool.LaunchUrl;
            obj.ResourceLink = new IResourceLinkMessage.ResourceLinkMessage
            {
                Id = scope.ResourceLink.Id,
                Description = scope.ResourceLink.Text,
                Title = scope.ResourceLink.Title
            };

            await Task.CompletedTask;
        }
    }
}
