using NP.Lti13Platform.Core.Models;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.MessageClaims;

/// <summary>
/// Defines the contract for a resource link message in LTI 1.3.
/// </summary>
public interface IResourceLinkClaims
{
    /// <summary>
    /// Gets or sets the resource link information.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/resource_link")]
    public ResourceLinkClaim ResourceLink { get; set; }

    /// <summary>
    /// Represents resource link information in an LTI message.
    /// </summary>
    public class ResourceLinkClaim
    {
        /// <summary>
        /// Gets or sets the resource link Id.
        /// </summary>
        [JsonPropertyName("id")]
        public required ResourceLinkId Id { get; set; }

        /// <summary>
        /// Gets or sets the resource link description.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the resource link title.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }
}

public static partial class ClaimsExtensions
{
    /// <summary>
    /// Populates the resource link claim properties of the specified object using the provided resource link.
    /// </summary>
    /// <remarks>This method sets the <see cref="IResourceLinkClaims.ResourceLink"/> property of <paramref name="obj"/> to a new claim based on the values from <paramref name="resourceLink"/>.
    /// The original object is returned to support fluent chaining.</remarks>
    /// <typeparam name="T">The type of object that implements <see cref="IResourceLinkClaims"/> and will have its resource link claim populated.</typeparam>
    /// <param name="obj">The object whose <see cref="IResourceLinkClaims.ResourceLink"/> property will be set. Must not be <see langword="null"/>.</param>
    /// <param name="resourceLink">The resource link containing the values to assign to the object's resource link claim. Must not be <see langword="null"/>.</param>
    /// <returns>The same object instance with its resource link claim properties set to match the provided resource link.</returns>
    public static T WithResourceLinkClaims<T>(
        this T obj,
        ResourceLink resourceLink)
        where T : IResourceLinkClaims
    {
        obj.ResourceLink = new IResourceLinkClaims.ResourceLinkClaim
        {
            Id = resourceLink.Id,
            Description = resourceLink.Text,
            Title = resourceLink.Title
        };

        return obj;
    }
}