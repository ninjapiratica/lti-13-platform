﻿using System.Text.Json.Serialization;
using NP.Lti13Platform.Core.Services;

namespace NP.Lti13Platform.Core.Populators;

public interface IPlatformMessage
{
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/tool_platform")]
    public ToolPlatform? Platform { get; set; }

    public class ToolPlatform
    {
        [JsonPropertyName("guid")]
        public required string Guid { get; set; }

        [JsonPropertyName("contact_email")]
        public string? ContactEmail { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("product_family_code")]
        public string? ProductFamilyCode { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }
    }
}

public class PlatformPopulator(ILti13PlatformService platformService) : Populator<IPlatformMessage>
{
    public override async Task PopulateAsync(IPlatformMessage obj, MessageScope scope, CancellationToken cancellationToken = default)
    {
        var platform = await platformService.GetPlatformAsync(scope.Tool.ClientId, cancellationToken);

        if (platform != null)
        {
            obj.Platform = new IPlatformMessage.ToolPlatform
            {
                Guid = platform.Guid,
                ContactEmail = platform.ContactEmail,
                Description = platform.Description,
                Name = platform.Name,
                ProductFamilyCode = platform.ProductFamilyCode,
                Url = platform.Url?.ToString(),
                Version = platform.Version,
            };
        }
    }
}
