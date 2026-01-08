using NP.Lti13Platform.Core.Models;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.MessageClaims;

/// <summary>
/// Defines the contract for a message containing LTI platform information.
/// </summary>
public interface IPlatformInstanceClaims
{
    /// <summary>
    /// Gets or sets the tool platform information.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/tool_platform")]
    public PlatformInstanceClaim? Platform { get; set; }

    /// <summary>
    /// Represents the tool platform information.
    /// </summary>
    public class PlatformInstanceClaim
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

public static partial class ClaimsExtensions
{
    /// <summary>
    /// Populates the platform-related claim properties of the specified object using the values from the provided platform.
    /// </summary>
    /// <remarks>This method sets the <c>Platform</c> property of the object to a new <c>PlatformInstanceClaim</c> containing the platform's details. The original object is returned to support fluent usage.</remarks>
    /// <typeparam name="T">The type of the object to populate. Must implement <see cref="IPlatformInstanceClaims"/>.</typeparam>
    /// <param name="obj">The object whose platform claim properties will be set. Must implement <see cref="IPlatformInstanceClaims"/>.</param>
    /// <param name="platform">The platform whose details are used to populate the claim properties. Cannot be null.</param>
    /// <returns>The same object instance with its platform claim properties set to match the provided platform.</returns>
    public static T WithPlatformInstanceClaims<T>(
        this T obj,
        Platform platform)
        where T : IPlatformInstanceClaims
    {
        obj.Platform = new IPlatformInstanceClaims.PlatformInstanceClaim
        {
            ContactEmail = platform.ContactEmail,
            Description = platform.Description,
            Guid = platform.Guid,
            Name = platform.Name,
            ProductFamilyCode = platform.ProductFamilyCode,
            Url = platform.Url?.OriginalString,
            Version = platform.Version,
        };

        return obj;
    }
}
