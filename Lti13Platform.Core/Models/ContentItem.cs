using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace NP.Lti13Platform.Models
{
    public class ContentItemDictionary() : IDictionary<(Guid?, string), Type>
    {
        private readonly IDictionary<(Guid?, string), Type> _items = new Dictionary<(Guid?, string), Type>();

        public Type this[(Guid?, string) key]
        {
            get => _items.TryGetValue(key, out Type? value) ? value : key.Item1 != null && _items.TryGetValue((null, key.Item2), out value) ? value : typeof(DefaultContentItem);
            set => _items[key] = value;
        }

        public ICollection<(Guid?, string)> Keys => _items.Keys;

        public ICollection<Type> Values => _items.Values;

        public int Count => _items.Count;

        public bool IsReadOnly => _items.IsReadOnly;

        public void Add((Guid?, string) key, Type value) => _items[key] = value;

        public void Add(KeyValuePair<(Guid?, string), Type> item) => _items[item.Key] = item.Value;

        public void Clear() => _items.Clear();

        public bool Contains(KeyValuePair<(Guid?, string), Type> item) => _items.Contains(item);

        public bool ContainsKey((Guid?, string) key) => _items.ContainsKey(key);

        public void CopyTo(KeyValuePair<(Guid?, string), Type>[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<(Guid?, string), Type>> GetEnumerator() => _items.GetEnumerator();

        public bool Remove((Guid?, string) key) => _items.Remove(key);

        public bool Remove(KeyValuePair<(Guid?, string), Type> item) => _items.Remove(item.Key);

        public bool TryGetValue((Guid?, string) key, [MaybeNullWhen(false)] out Type value)
        {
            value = this[key];
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }

    public static class ContentItemType
    {
        public static readonly string Html = "html";
        public static readonly string Link = "link";
        public static readonly string LtiResourceLink = "ltiResourceLink";
        public static readonly string File = "file";
        public static readonly string Image = "image";
    }

    public abstract class ContentItem
    {
        public required Guid Id { get; set; }
        public required string Type { get; set; }
        public required Guid DeploymentId { get; set; }
        public Guid? ContextId { get; set; }
    }

    public class LinkContentItem : ContentItem
    {
        public required string Url { get; set; }
        public string? Title { get; set; }
        public string? Text { get; set; }
        public ContentItemIcon? Icon { get; set; }
        public ContentItemThumbnail? Thumbnail { get; set; }
        public ContentItemWindow? Window { get; set; }
        public LinkIframe? Iframe { get; set; }
        public LinkEmbed? Embed { get; set; }

        public class LinkIframe
        {
            public int? Width { get; set; }
            public int? Height { get; set; }
            public string? Src { get; set; }
        }

        public class LinkEmbed
        {
            public required string Html { get; set; }
        }
    }

    public class LtiResourceLinkContentItem : ContentItem
    {
        public string? Url { get; set; }
        public string? Title { get; set; }
        public string? Text { get; set; }
        public ContentItemIcon? Icon { get; set; }
        public ContentItemThumbnail? Thumbnail { get; set; }
        public ContentItemWindow? Window { get; set; }
        public LtiResourceLinkIframe? Iframe { get; set; }
        public IDictionary<string, string>? Custom { get; set; }
        public LtiResourceLinkLineItem? LineItem { get; set; }
        public LtiResourceLinkAvailable? Available { get; set; }
        public LtiResourceLinkSubmission? Submission { get; set; }

        public class LtiResourceLinkIframe
        {
            public int? Width { get; set; }
            public int? Height { get; set; }
        }

        public class LtiResourceLinkLineItem
        {
            public string? Label { get; set; }
            public decimal ScoreMaximum { get; set; }
            public string? ResourceId { get; set; }
            public string? Tag { get; set; }
            public bool? GradesReleased { get; set; }
        }

        public class LtiResourceLinkAvailable
        {
            public DateTime? StartDateTime { get; set; }
            public DateTime? EndDateTime { get; set; }
        }

        public class LtiResourceLinkSubmission
        {
            public DateTime? StartDateTime { get; set; }
            public DateTime? EndDateTime { get; set; }
        }
    }

    public class FileContentItem : ContentItem
    {
        public required string Url { get; set; }
        public string? Title { get; set; }
        public string? Text { get; set; }
        public ContentItemIcon? Icon { get; set; }
        public ContentItemThumbnail? Thumbnail { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class HtmlContentItem : ContentItem
    {
        public required string Html { get; set; }
        public string? Title { get; set; }
        public string? Text { get; set; }
    }

    public class ImageContentItem : ContentItem
    {
        public required string Url { get; set; }
        public string? Title { get; set; }
        public string? Text { get; set; }
        public ContentItemIcon? Icon { get; set; }
        public ContentItemThumbnail? Thumbnail { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    public class DefaultContentItem : ContentItem, IDictionary<string, JsonElement>
    {
        private readonly IDictionary<string, JsonElement> _items = new Dictionary<string, JsonElement>();

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
            if (key == nameof(Type))
            {
                Type = value?.GetString() ?? string.Empty;
            }
        }
    }

    public class ContentItemWindow
    {
        public string? TargetName { get; set; }
        public string? WindowFeatures { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    public class ContentItemThumbnail
    {
        public required string Url { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    public class ContentItemIcon
    {
        public required string Url { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}