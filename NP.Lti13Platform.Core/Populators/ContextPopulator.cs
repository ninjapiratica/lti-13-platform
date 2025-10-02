using NP.Lti13Platform.Core.Models;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.Populators;

/// <summary>
/// Defines the contract for a message containing LTI context information.
/// </summary>
public interface IContextMessage
{
    /// <summary>
    /// Gets or sets the message context.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/context")]
    public MessageContext? Context { get; set; }

    /// <summary>
    /// Represents the LTI context information.
    /// </summary>
    public class MessageContext
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

/// <summary>
/// Populates the <see cref="IContextMessage"/> with context information.
/// </summary>
public class ContextPopulator() : Populator<IContextMessage>
{
    /// <inheritdoc />
    public override async Task PopulateAsync(IContextMessage obj, MessageScope scope, CancellationToken cancellationToken = default)
    {
        if (scope.Context != null)
        {
            obj.Context = new IContextMessage.MessageContext
            {
                Id = scope.Context.Id,
                Label = scope.Context.Label,
                Title = scope.Context.Title,
                Types = [.. scope.Context.Types]
            };
        }

        await Task.CompletedTask;
    }
}
