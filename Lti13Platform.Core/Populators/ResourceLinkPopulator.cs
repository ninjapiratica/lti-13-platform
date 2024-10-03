using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.Populators
{
    public interface IResourceLinkMessage : ILaunchPresentationMessage
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
            if (scope.ResourceLink == null)
            {
                // TODO: make more specific
                throw new Exception();
            }

            obj.LtiVersion = "1.3.0";
            obj.DeploymentId = scope.Deployment.Id;

            obj.TargetLinkUri = scope.ResourceLink.Url ?? scope.Tool.LaunchUrl;
            obj.ResourceLink = new IResourceLinkMessage.ResourceLinkMessage
            {
                Id = scope.ResourceLink.Id,
                Description = scope.ResourceLink.Text,
                Title = scope.ResourceLink.Title
            };

            if (!string.IsNullOrWhiteSpace(scope.MessageHint))
            {
                var  launchPresentation = JsonSerializer.Deserialize<LaunchPresentationOverride>(Encoding.UTF8.GetString(Convert.FromBase64String(scope.MessageHint)));
                obj.LaunchPresentation = new ILaunchPresentationMessage.LaunchPresentationDefinition
                {
                    DocumentTarget = launchPresentation?.DocumentTarget,
                    Height = launchPresentation?.Height,
                    Locale = launchPresentation?.Locale,
                    ReturnUrl = launchPresentation?.ReturnUrl,
                    Width = launchPresentation?.Width,
                };
            }

            await Task.CompletedTask;
        }
    }
}
