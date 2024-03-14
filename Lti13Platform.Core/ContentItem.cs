using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace NP.Lti13Platform
{
    public abstract class ContentItem(JsonElement element, IEnumerable<string> knownKeys) : IReadOnlyDictionary<string, JsonElement>
    {
        protected static JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true, };

        public static ContentItem Parse(JsonElement element)
        {
            var type = element.GetProperty("type").GetString();

            return type switch
            {
                "link" => new LinkContentItem(element),
                "ltiResourceLink" => new ResourceLinkContentItem(element),
                "file" => new FileContentItem(element),
                "html" => new HtmlContentItem(element),
                "image" => new ImageContentItem(element),
                _ => new CustomContentItem(element),
            };
        }

        private readonly IEnumerable<string> knownKeys = knownKeys.Append("type");

        public string Type => element.GetProperty("type").GetString()!;

        public IDictionary<string, JsonElement> AdditionalProperties => element.EnumerateObject().Where(x => !knownKeys.Contains(x.Name)).ToDictionary(x => x.Name, x => x.Value);

        #region IDictionary
        public bool ContainsKey(string key) => element.TryGetProperty(key, out _);
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out JsonElement value) => element.TryGetProperty(key, out value);
        public IEnumerator<KeyValuePair<string, JsonElement>> GetEnumerator() => element.EnumerateObject().ToDictionary(x => x.Name, x => x.Value).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => element.EnumerateObject();
        public IEnumerable<string> Keys => element.EnumerateObject().Select(e => e.Name);
        public IEnumerable<JsonElement> Values => element.EnumerateObject().Select(e => e.Value);
        public int Count => element.EnumerateObject().Count();
        public JsonElement this[string key] => element.GetProperty(key);
        #endregion
    }

    public class ContentItemEnumerator(JsonElement.ObjectEnumerator enumerator) : IEnumerator<KeyValuePair<string, JsonElement>>
    {
        public KeyValuePair<string, JsonElement> Current => new(enumerator.Current.Name, enumerator.Current.Value);
        object IEnumerator.Current => this.Current;

        public void Dispose() => enumerator.Dispose();

        public bool MoveNext() => enumerator.MoveNext();

        public void Reset() => enumerator.Reset();
    }

    public class LinkContentItem(JsonElement element) : ContentItem(element, ["url", "title", "text", "icon", "thumbnail", "window", "iframe", "embed"])
    {
        public string Url => this["url"].GetString()!;
        public string? Title => TryGetValue("title", out var x) ? x.GetString() : null;
        public string? Text => TryGetValue("text", out var x) ? x.GetString() : null;
        public ContentItemIcon? Icon => TryGetValue("icon", out var x) ? x.Deserialize<ContentItemIcon>(Options) : null;
        public ContentItemThumbnail? Thumbnail => TryGetValue("thumbnail", out var x) ? x.Deserialize<ContentItemThumbnail>(Options) : null;
        public ContentItemWindow? Window => TryGetValue("window", out var x) ? x.Deserialize<ContentItemWindow>(Options) : null;
        public ContentItemLInkIframe? Iframe => TryGetValue("iframe", out var x) ? x.Deserialize<ContentItemLInkIframe>(Options) : null;
        public ContentItemEmbed? Embed => TryGetValue("embed", out var x) ? x.Deserialize<ContentItemEmbed>(Options) : null;
    }

    public class ResourceLinkContentItem(JsonElement element) : ContentItem(element, ["url", "title", "text", "icon", "thumbnail", "window", "iframe", "custom", "lineItem", "available", "submission"])
    {
        public string? Url => TryGetValue("url", out var x) ? x.GetString() : null;
        public string? Title => TryGetValue("title", out var x) ? x.GetString() : null;
        public string? Text => TryGetValue("text", out var x) ? x.GetString() : null;
        public ContentItemIcon? Icon => TryGetValue("icon", out var x) ? x.Deserialize<ContentItemIcon>(Options) : null;
        public ContentItemThumbnail? Thumbnail => TryGetValue("thumbnail", out var x) ? x.Deserialize<ContentItemThumbnail>(Options) : null;
        public ContentItemWindow? Window => TryGetValue("window", out var x) ? x.Deserialize<ContentItemWindow>(Options) : null;
        public ContentItemResourceLinkIframe? Iframe => TryGetValue("iframe", out var x) ? x.Deserialize<ContentItemResourceLinkIframe>(Options) : null;
        public IDictionary<string, string>? Custom => TryGetValue("custom", out var x) ? x.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetString()!) : null;
        public ContentItemLineItem? LineItem => TryGetValue("lineItem", out var x) ? x.Deserialize<ContentItemLineItem>(Options) : null;
        public ContentItemAvailable? Available => TryGetValue("available", out var x) ? x.Deserialize<ContentItemAvailable>(Options) : null;
        public ContentItemSubmission? Submission => TryGetValue("submission", out var x) ? x.Deserialize<ContentItemSubmission>(Options) : null;
    }

    public class FileContentItem(JsonElement element) : ContentItem(element, ["url", "title", "text", "icon", "thumbnail", "expiresAt"])
    {
        public string Url => this["url"].GetString()!;
        public string? Title => TryGetValue("title", out var x) ? x.GetString() : null;
        public string? Text => TryGetValue("text", out var x) ? x.GetString() : null;
        public ContentItemIcon? Icon => TryGetValue("icon", out var x) ? x.Deserialize<ContentItemIcon>(Options) : null;
        public ContentItemThumbnail? Thumbnail => TryGetValue("thumbnail", out var x) ? x.Deserialize<ContentItemThumbnail>(Options) : null;
        public DateTime? ExpiresAt => TryGetValue("expiresAt", out var x) ? x.GetDateTime() : null;
    }

    public class HtmlContentItem(JsonElement element) : ContentItem(element, ["html", "title", "text"])
    {
        public string Html => this["html"].GetString()!;
        public string? Title => TryGetValue("title", out var x) ? x.GetString() : null;
        public string? Text => TryGetValue("text", out var x) ? x.GetString() : null;
    }

    public class ImageContentItem(JsonElement element) : ContentItem(element, ["url", "title", "text", "icon", "thumbnail", "width", "height"])
    {
        public string Url => this["url"].GetString()!;
        public string? Title => TryGetValue("title", out var x) ? x.GetString() : null;
        public string? Text => TryGetValue("text", out var x) ? x.GetString() : null;
        public ContentItemIcon? Icon => TryGetValue("icon", out var x) ? x.Deserialize<ContentItemIcon>(Options) : null;
        public ContentItemThumbnail? Thumbnail => TryGetValue("thumbnail", out var x) ? x.Deserialize<ContentItemThumbnail>(Options) : null;
        public int? Width => TryGetValue("width", out var x) ? x.GetInt32() : null;
        public int? Height => TryGetValue("height", out var x) ? x.GetInt32() : null;
    }

    public class CustomContentItem(JsonElement element) : ContentItem(element, [])
    {
    }

    public class ContentItemSubmission
    {
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }

    public class ContentItemAvailable
    {
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }

    public class ContentItemLineItem
    {
        public string? Label { get; set; }
        public decimal ScoreMaximum { get; set; }
        public string? ResourceId { get; set; }
        public string? Tag { get; set; }
        public bool? GradesReleased { get; set; }
    }

    public class ContentItemResourceLinkIframe
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    public class ContentItemLInkIframe
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Src { get; set; }
    }

    public class ContentItemWindow
    {
        public string? TargetName { get; set; }
        public string? WindowFeatures { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    public class ContentItemEmbed
    {
        public string Html { get; set; }
    }

    public class ContentItemThumbnail
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ContentItemIcon
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

}