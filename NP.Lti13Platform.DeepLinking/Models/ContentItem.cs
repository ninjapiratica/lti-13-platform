using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.DeepLinking.Models
{
    public class ContentItemDictionary() : IDictionary<(string? ToolId, string ContentItemType), Type>
    {
        private readonly IDictionary<(string?, string), Type> _items = new Dictionary<(string?, string), Type>();

        public Type this[(string? ToolId, string ContentItemType) key]
        {
            get => _items.TryGetValue(key, out Type? value) ? value : key.ToolId != null && _items.TryGetValue((null, key.ContentItemType), out value) ? value : typeof(DefaultContentItem);
            set => _items[key] = value;
        }

        public ICollection<(string?, string)> Keys => _items.Keys;

        public ICollection<Type> Values => _items.Values;

        public int Count => _items.Count;

        public bool IsReadOnly => _items.IsReadOnly;

        public void Add((string?, string) key, Type value) => _items[key] = value;

        public void Add(KeyValuePair<(string?, string), Type> item) => _items[item.Key] = item.Value;

        public void Clear() => _items.Clear();

        public bool Contains(KeyValuePair<(string?, string), Type> item) => _items.Contains(item);

        public bool ContainsKey((string?, string) key) => _items.ContainsKey(key);

        public void CopyTo(KeyValuePair<(string?, string), Type>[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<(string?, string), Type>> GetEnumerator() => _items.GetEnumerator();

        public bool Remove((string?, string) key) => _items.Remove(key);

        public bool Remove(KeyValuePair<(string?, string), Type> item) => _items.Remove(item.Key);

        public bool TryGetValue((string?, string) key, [MaybeNullWhen(false)] out Type value)
        {
            value = this[key];
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }

    public static class ContentItemType
    {
        public const string Html = "html";
        public const string Link = "link";
        public const string LtiResourceLink = "ltiResourceLink";
        public const string File = "file";
        public const string Image = "image";
    }

    [JsonDerivedType(typeof(LinkContentItem))]
    [JsonDerivedType(typeof(LtiResourceLinkContentItem))]
    [JsonDerivedType(typeof(FileContentItem))]
    [JsonDerivedType(typeof(HtmlContentItem))]
    [JsonDerivedType(typeof(ImageContentItem))]
    [JsonDerivedType(typeof(DefaultContentItem))]
    public abstract partial class ContentItem
    {
        [JsonPropertyName("type")]
        public required string Type { get; set; }
    }

    public class LinkContentItem : ContentItem
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("icon")]
        public ContentItemIcon? Icon { get; set; }

        [JsonPropertyName("thumbnail")]
        public ContentItemThumbnail? Thumbnail { get; set; }

        [JsonPropertyName("window")]
        public ContentItemWindow? Window { get; set; }

        [JsonPropertyName("iframe")]
        public LinkIframe? Iframe { get; set; }

        [JsonPropertyName("embed")]
        public LinkEmbed? Embed { get; set; }

        public class LinkIframe
        {
            [JsonPropertyName("width")]
            public int? Width { get; set; }

            [JsonPropertyName("height")]
            public int? Height { get; set; }

            [JsonPropertyName("src")]
            public string? Src { get; set; }
        }

        public class LinkEmbed
        {
            [JsonPropertyName("html")]
            public required string Html { get; set; }
        }
    }

    public class LtiResourceLinkContentItem : ContentItem
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("icon")]
        public ContentItemIcon? Icon { get; set; }

        [JsonPropertyName("thumbnail")]
        public ContentItemThumbnail? Thumbnail { get; set; }

        [JsonPropertyName("window")]
        public ContentItemWindow? Window { get; set; }

        [JsonPropertyName("iframe")]
        public LtiResourceLinkIframe? Iframe { get; set; }

        [JsonPropertyName("custom")]
        public IDictionary<string, string>? Custom { get; set; }

        [JsonPropertyName("lineItem")]
        public LtiResourceLinkLineItem? LineItem { get; set; }

        [JsonPropertyName("available")]
        public LtiResourceLinkAvailable? Available { get; set; }

        [JsonPropertyName("submission")]
        public LtiResourceLinkSubmission? Submission { get; set; }

        public class LtiResourceLinkIframe
        {
            [JsonPropertyName("width")]
            public int? Width { get; set; }

            [JsonPropertyName("height")]
            public int? Height { get; set; }
        }

        public class LtiResourceLinkLineItem
        {
            [JsonPropertyName("label")]
            public string? Label { get; set; }

            [JsonPropertyName("scoreMaximum")]
            public decimal ScoreMaximum { get; set; }

            [JsonPropertyName("resourceId")]
            public string? ResourceId { get; set; }

            [JsonPropertyName("tag")]
            public string? Tag { get; set; }

            [JsonPropertyName("gradesReleased")]
            public bool? GradesReleased { get; set; }
        }

        public class LtiResourceLinkAvailable
        {
            [JsonPropertyName("startDateTime")]
            public DateTime? StartDateTime { get; set; }

            [JsonPropertyName("endDateTime")]
            public DateTime? EndDateTime { get; set; }
        }

        public class LtiResourceLinkSubmission
        {
            [JsonPropertyName("startDateTime")]
            public DateTime? StartDateTime { get; set; }

            [JsonPropertyName("endDateTime")]
            public DateTime? EndDateTime { get; set; }
        }
    }

    public class FileContentItem : ContentItem
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("icon")]
        public ContentItemIcon? Icon { get; set; }

        [JsonPropertyName("thumbnail")]
        public ContentItemThumbnail? Thumbnail { get; set; }

        [JsonPropertyName("expiresAt")]
        public DateTime? ExpiresAt { get; set; }
    }

    public class HtmlContentItem : ContentItem
    {
        [JsonPropertyName("html")]
        public required string Html { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class ImageContentItem : ContentItem
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("icon")]
        public ContentItemIcon? Icon { get; set; }

        [JsonPropertyName("thumbnail")]
        public ContentItemThumbnail? Thumbnail { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }
    }

    public class DefaultContentItem : ContentItem, IDictionary<string, JsonElement>
    {
        private readonly IDictionary<string, JsonElement> _items = new Dictionary<string, JsonElement>();
        private static readonly string TypePropertyName = typeof(ContentItem).GetProperty(nameof(Type))?.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? string.Empty;

        public JsonElement this[string key]
        {
            get => _items[key];
            set
            {
                SetKnownProperty(key, value);
                _items[key] = value;
            }
        }

        public ICollection<string> Keys => _items.Keys;

        public ICollection<JsonElement> Values => _items.Values;

        public int Count => _items.Count;

        public bool IsReadOnly => _items.IsReadOnly;

        public void Add(string key, JsonElement value)
        {
            SetKnownProperty(key, value);
            _items.Add(key, value);
        }

        public void Add(KeyValuePair<string, JsonElement> item)
        {
            SetKnownProperty(item.Key, item.Value);
            _items.Add(item);
        }

        public void Clear()
        {
            Type = string.Empty;
            _items.Clear();
        }

        public bool Contains(KeyValuePair<string, JsonElement> item) => _items.Contains(item);

        public bool ContainsKey(string key) => _items.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, JsonElement>[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<string, JsonElement>> GetEnumerator() => _items.GetEnumerator();

        public bool Remove(string key)
        {
            if (_items.Remove(key))
            {
                SetKnownProperty(key, null);
                return true;
            }

            return false;
        }

        public bool Remove(KeyValuePair<string, JsonElement> item)
        {
            if (_items.Remove(item.Key))
            {
                SetKnownProperty(item.Key, null);
                return true;
            }

            return false;
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out JsonElement value) => _items.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        private void SetKnownProperty(string key, JsonElement? value)
        {
            if (key == TypePropertyName)
            {
                Type = value?.GetString() ?? string.Empty;
            }
        }
    }

    public class ContentItemWindow
    {
        [JsonPropertyName("targetName")]
        public string? TargetName { get; set; }

        [JsonPropertyName("windowFeatures")]
        public string? WindowFeatures { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }
    }

    public class ContentItemThumbnail
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }
    }

    public class ContentItemIcon
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }
    }
}