using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using NP.Lti13Platform.Core;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.DeepLinking
{
    public interface IDeepLinkingMessage
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/version")]
        string LtiVersion { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/deployment_id")]
        string DeploymentId { get; set; }

        [JsonPropertyName("https://purl.imsglobal.org/spec/lti-dl/claim/deep_linking_settings")]
        DeepLinkSettingsMessage DeepLinkSettings { get; set; }

        public class DeepLinkSettingsMessage
        {
            [JsonPropertyName("deep_link_return_url")]
            public required string DeepLinkReturnUrl { get; set; }

            [JsonPropertyName("accept_types")]
            public required IEnumerable<string> AcceptTypes { get; set; }

            [JsonPropertyName("accept_presentation_document_targets")]
            public required IEnumerable<string> AcceptPresentationDocumentTargets { get; set; }

            [JsonPropertyName("accept_media_types")]
            public string? AcceptMediaTypesSerialized => AcceptMediaTypes == null ? null : string.Join(",", AcceptMediaTypes);

            [JsonIgnore]
            public IEnumerable<string>? AcceptMediaTypes { get; set; }

            [JsonPropertyName("accept_multiple")]
            public bool? AcceptMultiple { get; set; }

            [JsonPropertyName("accept_lineitem")]
            public bool? AcceptLineItem { get; set; }

            [JsonPropertyName("auto_create")]
            public bool? AutoCreate { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("text")]
            public string? Text { get; set; }

            [JsonPropertyName("data")]
            public string? Data { get; set; }
        }
    }

    public class PopulateDeepLinking(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator, IOptionsMonitor<Lti13PlatformConfig> config) : Populator<IDeepLinkingMessage>
    {
        public override async Task Populate(IDeepLinkingMessage obj, Lti13MessageScope scope)
        {
            obj.LtiVersion = "1.3.0";
            obj.DeploymentId = scope.Deployment.Id;

            IDeepLinkingMessage.DeepLinkSettingsMessage? deepLinkSettings = default;

            if (!string.IsNullOrWhiteSpace(scope.MessageHint))
            {
                var parts = Encoding.UTF8.GetString(Convert.FromBase64String(scope.MessageHint)).Split('|');
                deepLinkSettings = JsonSerializer.Deserialize<IDeepLinkingMessage.DeepLinkSettingsMessage>(parts[0])!;
            }

            obj.DeepLinkSettings = new IDeepLinkingMessage.DeepLinkSettingsMessage
            {
                AcceptPresentationDocumentTargets = new[] { deepLinkSettings?.AcceptPresentationDocumentTargets, config.CurrentValue.DeepLink.AcceptPresentationDocumentTargets }.FirstOrDefault(x => x != null && x.Any()) ?? [],
                AcceptTypes = new[] { deepLinkSettings?.AcceptTypes, config.CurrentValue.DeepLink.AcceptTypes }.FirstOrDefault(x => x != null && x.Any()) ?? [],
                DeepLinkReturnUrl = linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, RouteNames.DEEP_LINKING_RESPONSE, new { contextId = scope.Context?.Id }) ?? string.Empty,
                AcceptLineItem = deepLinkSettings?.AcceptLineItem ?? config.CurrentValue.DeepLink.AcceptLineItem,
                AcceptMediaTypes = new[] { deepLinkSettings?.AcceptMediaTypes, config.CurrentValue.DeepLink.AcceptMediaTypes }.FirstOrDefault(x => x != null && x.Any()),
                AcceptMultiple = deepLinkSettings?.AcceptMultiple ?? config.CurrentValue.DeepLink.AcceptMultiple,
                AutoCreate = deepLinkSettings?.AutoCreate ?? config.CurrentValue.DeepLink.AutoCreate,
                Data = deepLinkSettings?.Data,
                Text = deepLinkSettings?.Text,
                Title = deepLinkSettings?.Title,
            };

            await Task.CompletedTask;
        }
    }
}
