using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.Populators
{
    public interface IContextMessage
    {
        [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/context")]
        public MessageContext? Context { get; set; }

        public class MessageContext
        {
            [JsonPropertyName("id")]
            public required string Id { get; set; }

            [JsonPropertyName("label")]
            public string? Label { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("type")]
            public IEnumerable<string> Types { get; set; } = [];
        }
    }

    public class ContextPopulator() : Populator<IContextMessage>
    {
        public override async Task PopulateAsync(IContextMessage obj, MessageScope scope, CancellationToken cancellationToken = default)
        {
            if (scope.Context != null)
            {
                obj.Context = new IContextMessage.MessageContext
                {
                    Id = scope.Context.Id,
                    Label = scope.Context.Label,
                    Title = scope.Context.Title,
                    Types = scope.Context.Types.ToArray()
                };
            }

            await Task.CompletedTask;
        }
    }
}
