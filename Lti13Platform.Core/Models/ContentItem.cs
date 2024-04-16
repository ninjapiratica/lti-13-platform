using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace NP.Lti13Platform.Models
{
    public abstract class ContentItem(JsonElement element, IEnumerable<string> knownKeys) : IReadOnlyDictionary<string, JsonElement>
    {
        protected const string URL = "url";
        protected const string TITLE = "title";
        protected const string TEXT = "text";
        protected const string ICON = "icon";
        protected const string THUMBNAIL = "thumbnail";
        protected const string WIDTH = "width";
        protected const string HEIGHT = "height";
        protected const string HTML = "html";
        protected const string EXPIRES_AT = "expiresAt";
        protected const string WINDOW = "window";
        protected const string IFRAME = "iframe";
        protected const string EMBED = "embed";
        protected const string CUSTOM = "custom";
        protected const string LINE_ITEM = "lineItem";
        protected const string AVAILABLE = "available";
        protected const string SUBMISSION = "submission";
        protected const string TYPE = "type";

        private const string LINK = "link";
        private const string LTI_RESOURCE_LINK = "ltiResourceLink";
        private const string FILE = "file";
        private const string IMAGE = "image";

        protected readonly static JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true, };

        public static ContentItem Parse(JsonElement element)
        {
            var type = element.GetProperty(TYPE).GetString();

            return type switch
            {
                LINK => new LinkContentItem(element),
                LTI_RESOURCE_LINK => new ResourceLinkContentItem(element),
                FILE => new FileContentItem(element),
                HTML => new HtmlContentItem(element),
                IMAGE => new ImageContentItem(element),
                _ => new CustomContentItem(element),
            };
        }

        private readonly IEnumerable<string> knownKeys = knownKeys.Append(TYPE);

        public string Type => element.GetProperty(TYPE).GetString()!;

        public IDictionary<string, JsonElement> AdditionalProperties => element.EnumerateObject().Where(x => !knownKeys.Contains(x.Name)).ToDictionary(x => x.Name, x => x.Value);

        #region IDictionary
        public bool ContainsKey(string key) => element.TryGetProperty(key, out _);
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out JsonElement value) => element.TryGetProperty(key, out value);
        public IEnumerator<KeyValuePair<string, JsonElement>> GetEnumerator() => new ContentItemEnumerator(element.EnumerateObject().GetEnumerator());
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

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            enumerator.Dispose();
            GC.SuppressFinalize(this);
        }

        public bool MoveNext() => enumerator.MoveNext();

        public void Reset() => enumerator.Reset();
    }

    public class LinkContentItem(JsonElement element) : ContentItem(element, [URL, TITLE, TEXT, ICON, THUMBNAIL, WINDOW, IFRAME, EMBED])
    {
        public string Url => this[URL].GetString()!;
        public string? Title => TryGetValue(TITLE, out var x) ? x.GetString() : null;
        public string? Text => TryGetValue(TEXT, out var x) ? x.GetString() : null;
        public ContentItemIcon? Icon => TryGetValue(ICON, out var x) ? x.Deserialize<ContentItemIcon>(Options) : null;
        public ContentItemThumbnail? Thumbnail => TryGetValue(THUMBNAIL, out var x) ? x.Deserialize<ContentItemThumbnail>(Options) : null;
        public ContentItemWindow? Window => TryGetValue(WINDOW, out var x) ? x.Deserialize<ContentItemWindow>(Options) : null;
        public ContentItemLInkIframe? Iframe => TryGetValue(IFRAME, out var x) ? x.Deserialize<ContentItemLInkIframe>(Options) : null;
        public ContentItemEmbed? Embed => TryGetValue(EMBED, out var x) ? x.Deserialize<ContentItemEmbed>(Options) : null;
    }

    public class ResourceLinkContentItem(JsonElement element) : ContentItem(element, [URL, TITLE, TEXT, ICON, THUMBNAIL, WINDOW, IFRAME, CUSTOM, LINE_ITEM, AVAILABLE, SUBMISSION])
    {
        public Guid Id => Guid.NewGuid(); // TODO: Figure this out
        public Guid ContextId => Guid.NewGuid(); // TODO: Figure this out
        public string? Url => TryGetValue(URL, out var x) ? x.GetString() : null;
        public string? Title => TryGetValue(TITLE, out var x) ? x.GetString() : null;
        public string? Text => TryGetValue(TEXT, out var x) ? x.GetString() : null;
        public ContentItemIcon? Icon => TryGetValue(ICON, out var x) ? x.Deserialize<ContentItemIcon>(Options) : null;
        public ContentItemThumbnail? Thumbnail => TryGetValue(THUMBNAIL, out var x) ? x.Deserialize<ContentItemThumbnail>(Options) : null;
        public ContentItemWindow? Window => TryGetValue(WINDOW, out var x) ? x.Deserialize<ContentItemWindow>(Options) : null;
        public ContentItemResourceLinkIframe? Iframe => TryGetValue(IFRAME, out var x) ? x.Deserialize<ContentItemResourceLinkIframe>(Options) : null;
        public IDictionary<string, string>? Custom => TryGetValue(CUSTOM, out var x) ? x.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetString()!) : null;
        public ContentItemLineItem? LineItem => TryGetValue(LINE_ITEM, out var x) ? x.Deserialize<ContentItemLineItem>(Options) : null;
        public ContentItemAvailable? Available => TryGetValue(AVAILABLE, out var x) ? x.Deserialize<ContentItemAvailable>(Options) : null;
        public ContentItemSubmission? Submission => TryGetValue(SUBMISSION, out var x) ? x.Deserialize<ContentItemSubmission>(Options) : null;
    }

    public class FileContentItem(JsonElement element) : ContentItem(element, [URL, TITLE, TEXT, ICON, THUMBNAIL, EXPIRES_AT])
    {
        public string Url => this[URL].GetString()!;
        public string? Title => TryGetValue(TITLE, out var x) ? x.GetString() : null;
        public string? Text => TryGetValue(TEXT, out var x) ? x.GetString() : null;
        public ContentItemIcon? Icon => TryGetValue(ICON, out var x) ? x.Deserialize<ContentItemIcon>(Options) : null;
        public ContentItemThumbnail? Thumbnail => TryGetValue(THUMBNAIL, out var x) ? x.Deserialize<ContentItemThumbnail>(Options) : null;
        public DateTime? ExpiresAt => TryGetValue(EXPIRES_AT, out var x) ? x.GetDateTime() : null;
    }

    public class HtmlContentItem(JsonElement element) : ContentItem(element, [HTML, TITLE, TEXT])
    {
        public string Html => this[HTML].GetString()!;
        public string? Title => TryGetValue(TITLE, out var x) ? x.GetString() : null;
        public string? Text => TryGetValue(TEXT, out var x) ? x.GetString() : null;
    }

    public class ImageContentItem(JsonElement element) : ContentItem(element, [URL, TITLE, TEXT, ICON, THUMBNAIL, WIDTH, HEIGHT])
    {
        public string Url => this[URL].GetString()!;
        public string? Title => TryGetValue(TITLE, out var x) ? x.GetString() : null;
        public string? Text => TryGetValue(TEXT, out var x) ? x.GetString() : null;
        public ContentItemIcon? Icon => TryGetValue(ICON, out var x) ? x.Deserialize<ContentItemIcon>(Options) : null;
        public ContentItemThumbnail? Thumbnail => TryGetValue(THUMBNAIL, out var x) ? x.Deserialize<ContentItemThumbnail>(Options) : null;
        public int? Width => TryGetValue(WIDTH, out var x) ? x.GetInt32() : null;
        public int? Height => TryGetValue(HEIGHT, out var x) ? x.GetInt32() : null;
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
        public string Html { get; set; } = string.Empty;
    }

    public class ContentItemThumbnail
    {
        public string Url { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ContentItemIcon
    {
        public string Url { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
    }
}