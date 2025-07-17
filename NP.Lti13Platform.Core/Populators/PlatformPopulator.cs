using System.Text.Json.Serialization;
using NP.Lti13Platform.Core.Services;

namespace NP.Lti13Platform.Core.Populators;

/// <summary>
/// Defines the contract for a message containing LTI platform information.
/// </summary>
public interface IPlatformMessage
{
    /// <summary>
    /// Gets or sets the tool platform information.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/tool_platform")]
    public ToolPlatform? Platform { get; set; }

    /// <summary>
    /// Represents the tool platform information.
    /// </summary>
    public class ToolPlatform
    {
        /// <summary>
        /// Gets or sets the GUID of the platform.
        /// </summary>
        [JsonPropertyName("guid")]
        public required string Guid { get; set; }

        /// <summary>
        /// Gets or sets the contact email of the platform.
        /// </summary>
        [JsonPropertyName("contact_email")]
        public string? ContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the description of the platform.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the platform.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the URL of the platform.
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the product family code of the platform.
        /// </summary>
        [JsonPropertyName("product_family_code")]
        public string? ProductFamilyCode { get; set; }

        /// <summary>
        /// Gets or sets the version of the platform.
        /// </summary>
        [JsonPropertyName("version")]
        public string? Version { get; set; }
    }
}

/// <summary>
/// Populates the <see cref="IPlatformMessage"/> with platform information.
/// </summary>
/// <param name="platformService">The platform service.</param>
public class PlatformPopulator(ILti13PlatformService platformService) : Populator<IPlatformMessage>
{
    /// <inheritdoc />
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
