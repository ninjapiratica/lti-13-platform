using NP.Lti13Platform.Core.Models;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.MessageClaims;

/// <summary>
/// Defines the contract for a message containing LTI 1.3 context information.
/// </summary>
public interface IContextClaims
{
    /// <summary>
    /// Gets or sets the message context.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/context")]
    public ContextClaim? Context { get; set; }

    /// <summary>
    /// Represents the LTI 1.3 context information.
    /// </summary>
    public class ContextClaim
    {
        /// <summary>
        /// Gets or sets the ID of the context.
        /// </summary>
        [JsonPropertyName("id")]
        public required ContextId Id { get; set; }

        /// <summary>
        /// Gets or sets the label of the context.
        /// </summary>
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the title of the context.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the types of the context.
        /// </summary>
        [JsonPropertyName("type")]
        public IEnumerable<string> Types { get; set; } = [];
    }
}

public static partial class ClaimsExtensions
{
    /// <summary>
    /// Populates the context claims of the specified object using values from the provided context.
    /// </summary>
    /// <remarks>This method is intended to simplify the process of assigning context-related metadata to
    /// objects that support context claims. The original object is returned to support method chaining.</remarks>
    /// <typeparam name="T">The type of the object that implements the IContextClaims interface.</typeparam>
    /// <param name="obj">The object whose context claims will be filled. Must implement IContextClaims.</param>
    /// <param name="context">The context containing the values to assign to the object's context claims.</param>
    /// <returns>The same object instance with its context claims populated from the specified context.</returns>
    public static T WithContextClaims<T>(
        this T obj,
        Context context)
        where T : IContextClaims
    {
        obj.Context = new IContextClaims.ContextClaim
        {
            Id = context.Id,
            Label = context.Label,
            Title = context.Title,
            Types = [.. context.Types]
        };

        return obj;
    }
}
