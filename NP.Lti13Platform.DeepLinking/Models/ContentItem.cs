using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.DeepLinking.Models;

/// <summary>
/// Represents a dictionary for content items with a specific tool Id and content item type.
/// </summary>
public class ContentItemDictionary() : IDictionary<(string? ToolId, string ContentItemType), Type>
{
    private readonly IDictionary<(string?, string), Type> _items = new Dictionary<(string?, string), Type>();

    /// <summary>
    /// Gets or sets the Type associated with the specified key.
    /// If the key is not found but has a non-null ToolId, attempts to find a Type with the same ContentItemType but null ToolId.
    /// If no match is found, returns DefaultContentItem type.
    /// </summary>
    /// <param name="key">The key containing ToolId and ContentItemType.</param>
    /// <returns>The Type associated with the specified key, or DefaultContentItem if not found.</returns>
    public Type this[(string? ToolId, string ContentItemType) key]
    {
        get => _items.TryGetValue(key, out Type? value) ? value : key.ToolId != null && _items.TryGetValue((null, key.ContentItemType), out value) ? value : typeof(DefaultContentItem);
        set => _items[key] = value;
    }

    /// <summary>
    /// Gets a collection containing the keys in the dictionary.
    /// </summary>
    public ICollection<(string?, string)> Keys => _items.Keys;

    /// <summary>
    /// Gets a collection containing the values in the dictionary.
    /// </summary>
    public ICollection<Type> Values => _items.Values;

    /// <summary>
    /// Gets the number of key/value pairs contained in the dictionary.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Gets a value indicating whether the dictionary is read-only.
    /// </summary>
    public bool IsReadOnly => _items.IsReadOnly;

    /// <summary>
    /// Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    public void Add((string?, string) key, Type value) => _items[key] = value;

    /// <summary>
    /// Adds the specified key/value pair to the dictionary.
    /// </summary>
    /// <param name="item">The key/value pair to add.</param>
    public void Add(KeyValuePair<(string?, string), Type> item) => _items[item.Key] = item.Value;

    /// <summary>
    /// Removes all keys and values from the dictionary.
    /// </summary>
    public void Clear() => _items.Clear();

    /// <summary>
    /// Determines whether the dictionary contains a specific key/value pair.
    /// </summary>
    /// <param name="item">The key/value pair to locate in the dictionary.</param>
    /// <returns>True if the key/value pair is found in the dictionary; otherwise, false.</returns>
    public bool Contains(KeyValuePair<(string?, string), Type> item) => _items.Contains(item);

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns>True if the dictionary contains an element with the specified key; otherwise, false.</returns>
    public bool ContainsKey((string?, string) key) => _items.ContainsKey(key);

    /// <summary>
    /// Copies the elements of the dictionary to an array, starting at the specified array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the dictionary.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    public void CopyTo(KeyValuePair<(string?, string), Type>[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An enumerator for the dictionary.</returns>
    public IEnumerator<KeyValuePair<(string?, string), Type>> GetEnumerator() => _items.GetEnumerator();

    /// <summary>
    /// Removes the element with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>True if the element is successfully removed; otherwise, false.</returns>
    public bool Remove((string?, string) key) => _items.Remove(key);

    /// <summary>
    /// Removes the first occurrence of a specific key/value pair from the dictionary.
    /// </summary>
    /// <param name="item">The key/value pair to remove.</param>
    /// <returns>True if the key/value pair was successfully removed; otherwise, false.</returns>
    public bool Remove(KeyValuePair<(string?, string), Type> item) => _items.Remove(item.Key);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// Always returns true and sets the value to this[key], which may be DefaultContentItem if not found.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found;
    /// otherwise, the default value for the type of the value parameter.</param>
    /// <returns>Always returns true.</returns>
    public bool TryGetValue((string?, string) key, [MaybeNullWhen(false)] out Type value)
    {
        value = this[key];
        return true;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the dictionary.</returns>
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}

/// <summary>
/// Provides constants for different content item types.
/// </summary>
public static class ContentItemType
{
    public static readonly string Html = "html";
    public static readonly string Link = "link";
    public static readonly string LtiResourceLink = "ltiResourceLink";
    public static readonly string File = "file";
    public static readonly string Image = "image";
}

/// <summary>
/// Represents the base class for content items.
/// </summary>
[JsonDerivedType(typeof(LinkContentItem))]
[JsonDerivedType(typeof(LtiResourceLinkContentItem))]
[JsonDerivedType(typeof(FileContentItem))]
[JsonDerivedType(typeof(HtmlContentItem))]
[JsonDerivedType(typeof(ImageContentItem))]
[JsonDerivedType(typeof(DefaultContentItem))]
public abstract partial class ContentItem
{
    /// <summary>
    /// Gets or sets the type of the content item.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }
}

/// <summary>
/// Represents a link content item.
/// </summary>
public class LinkContentItem : ContentItem
{
    /// <summary>
    /// Gets or sets the URL of the link content item.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the title of the link content item.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the text of the link content item.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the icon of the link content item.
    /// </summary>
    [JsonPropertyName("icon")]
    public ContentItemIcon? Icon { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail of the link content item.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public ContentItemThumbnail? Thumbnail { get; set; }

    /// <summary>
    /// Gets or sets the window properties of the link content item.
    /// </summary>
    [JsonPropertyName("window")]
    public ContentItemWindow? Window { get; set; }

    /// <summary>
    /// Gets or sets the iframe properties of the link content item.
    /// </summary>
    [JsonPropertyName("iframe")]
    public LinkIframe? Iframe { get; set; }

    /// <summary>
    /// Gets or sets the embed properties of the link content item.
    /// </summary>
    [JsonPropertyName("embed")]
    public LinkEmbed? Embed { get; set; }

    /// <summary>
    /// Represents iframe properties for a link content item.
    /// </summary>
    public class LinkIframe
    {
        /// <summary>
        /// Gets or sets the width of the iframe.
        /// </summary>
        [JsonPropertyName("width")]
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the iframe.
        /// </summary>
        [JsonPropertyName("height")]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets the source URL of the iframe.
        /// </summary>
        [JsonPropertyName("src")]
        public string? Src { get; set; }
    }

    /// <summary>
    /// Represents embed properties for a link content item.
    /// </summary>
    public class LinkEmbed
    {
        /// <summary>
        /// Gets or sets the HTML content for embed.
        /// </summary>
        [JsonPropertyName("html")]
        public required string Html { get; set; }
    }
}

/// <summary>
/// Represents an LTI resource link content item.
/// </summary>
public class LtiResourceLinkContentItem : ContentItem
{
    /// <summary>
    /// Gets or sets the URL of the LTI resource link content item.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the title of the LTI resource link content item.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the text of the LTI resource link content item.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the icon of the LTI resource link content item.
    /// </summary>
    [JsonPropertyName("icon")]
    public ContentItemIcon? Icon { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail of the LTI resource link content item.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public ContentItemThumbnail? Thumbnail { get; set; }

    /// <summary>
    /// Gets or sets the window properties of the LTI resource link content item.
    /// </summary>
    [JsonPropertyName("window")]
    public ContentItemWindow? Window { get; set; }

    /// <summary>
    /// Gets or sets the iframe properties of the LTI resource link content item.
    /// </summary>
    [JsonPropertyName("iframe")]
    public LtiResourceLinkIframe? Iframe { get; set; }

    /// <summary>
    /// Gets or sets the custom parameters for the LTI resource link.
    /// </summary>
    [JsonPropertyName("custom")]
    public IDictionary<string, string>? Custom { get; set; }

    /// <summary>
    /// Gets or sets the line item properties for the LTI resource link.
    /// </summary>
    [JsonPropertyName("lineItem")]
    public LtiResourceLinkLineItem? LineItem { get; set; }

    /// <summary>
    /// Gets or sets the availability properties for the LTI resource link.
    /// </summary>
    [JsonPropertyName("available")]
    public LtiResourceLinkAvailable? Available { get; set; }

    /// <summary>
    /// Gets or sets the submission properties for the LTI resource link.
    /// </summary>
    [JsonPropertyName("submission")]
    public LtiResourceLinkSubmission? Submission { get; set; }

    /// <summary>
    /// Represents iframe properties for an LTI resource link content item.
    /// </summary>
    public class LtiResourceLinkIframe
    {
        /// <summary>
        /// Gets or sets the width of the iframe.
        /// </summary>
        [JsonPropertyName("width")]
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the iframe.
        /// </summary>
        [JsonPropertyName("height")]
        public int? Height { get; set; }
    }

    /// <summary>
    /// Represents line item properties for an LTI resource link content item.
    /// </summary>
    public class LtiResourceLinkLineItem
    {
        /// <summary>
        /// Gets or sets the label of the line item.
        /// </summary>
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the maximum score of the line item.
        /// </summary>
        [JsonPropertyName("scoreMaximum")]
        public decimal ScoreMaximum { get; set; }

        /// <summary>
        /// Gets or sets the resource ID of the line item.
        /// </summary>
        [JsonPropertyName("resourceId")]
        public string? ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the tag of the line item.
        /// </summary>
        [JsonPropertyName("tag")]
        public string? Tag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grades are released.
        /// </summary>
        [JsonPropertyName("gradesReleased")]
        public bool? GradesReleased { get; set; }
    }

    /// <summary>
    /// Represents availability properties for an LTI resource link content item.
    /// </summary>
    public class LtiResourceLinkAvailable
    {
        /// <summary>
        /// Gets or sets the start date and time of the availability period.
        /// </summary>
        [JsonPropertyName("startDateTime")]
        public DateTimeOffset? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date and time of the availability period.
        /// </summary>
        [JsonPropertyName("endDateTime")]
        public DateTimeOffset? EndDateTime { get; set; }
    }

    /// <summary>
    /// Represents submission properties for an LTI resource link content item.
    /// </summary>
    public class LtiResourceLinkSubmission
    {
        /// <summary>
        /// Gets or sets the start date and time of the submission period.
        /// </summary>
        [JsonPropertyName("startDateTime")]
        public DateTimeOffset? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date and time of the submission period.
        /// </summary>
        [JsonPropertyName("endDateTime")]
        public DateTimeOffset? EndDateTime { get; set; }
    }
}

/// <summary>
/// Represents a file content item.
/// </summary>
public class FileContentItem : ContentItem
{
    /// <summary>
    /// Gets or sets the URL of the file content item.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the title of the file content item.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the text of the file content item.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the icon of the file content item.
    /// </summary>
    [JsonPropertyName("icon")]
    public ContentItemIcon? Icon { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail of the file content item.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public ContentItemThumbnail? Thumbnail { get; set; }

    /// <summary>
    /// Gets or sets the expiration date and time of the file content item.
    /// </summary>
    [JsonPropertyName("expiresAt")]
    public DateTimeOffset? ExpiresAt { get; set; }
}

/// <summary>
/// Represents an HTML content item.
/// </summary>
public class HtmlContentItem : ContentItem
{
    /// <summary>
    /// Gets or sets the HTML content of the HTML content item.
    /// </summary>
    [JsonPropertyName("html")]
    public required string Html { get; set; }

    /// <summary>
    /// Gets or sets the title of the HTML content item.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the text of the HTML content item.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

/// <summary>
/// Represents an image content item.
/// </summary>
public class ImageContentItem : ContentItem
{
    /// <summary>
    /// Gets or sets the URL of the image content item.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the title of the image content item.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the text of the image content item.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the icon of the image content item.
    /// </summary>
    [JsonPropertyName("icon")]
    public ContentItemIcon? Icon { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail of the image content item.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public ContentItemThumbnail? Thumbnail { get; set; }

    /// <summary>
    /// Gets or sets the width of the image content item.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the image content item.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }
}

/// <summary>
/// Represents the default content item.
/// </summary>
public class DefaultContentItem : ContentItem, IDictionary<string, JsonElement>
{
    private readonly IDictionary<string, JsonElement> _items = new Dictionary<string, JsonElement>();
    private static readonly string TypePropertyName = typeof(ContentItem).GetProperty(nameof(Type))?.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? string.Empty;

    /// <summary>
    /// Gets or sets the JsonElement associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the element to get or set.</param>
    /// <returns>The JsonElement associated with the specified key.</returns>
    public JsonElement this[string key]
    {
        get => _items[key];
        set
        {
            SetKnownProperty(key, value);
            _items[key] = value;
        }
    }

    /// <summary>
    /// Gets a collection containing the keys in the dictionary.
    /// </summary>
    public ICollection<string> Keys => _items.Keys;

    /// <summary>
    /// Gets a collection containing the values in the dictionary.
    /// </summary>
    public ICollection<JsonElement> Values => _items.Values;

    /// <summary>
    /// Gets the number of key/value pairs contained in the dictionary.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Gets a value indicating whether the dictionary is read-only.
    /// </summary>
    public bool IsReadOnly => _items.IsReadOnly;

    /// <summary>
    /// Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    public void Add(string key, JsonElement value)
    {
        SetKnownProperty(key, value);
        _items.Add(key, value);
    }

    /// <summary>
    /// Adds the specified key/value pair to the dictionary.
    /// </summary>
    /// <param name="item">The key/value pair to add.</param>
    public void Add(KeyValuePair<string, JsonElement> item)
    {
        SetKnownProperty(item.Key, item.Value);
        _items.Add(item);
    }

    /// <summary>
    /// Removes all keys and values from the dictionary and clears the Type property.
    /// </summary>
    public void Clear()
    {
        Type = string.Empty;
        _items.Clear();
    }

    /// <summary>
    /// Determines whether the dictionary contains a specific key/value pair.
    /// </summary>
    /// <param name="item">The key/value pair to locate in the dictionary.</param>
    /// <returns>True if the key/value pair is found in the dictionary; otherwise, false.</returns>
    public bool Contains(KeyValuePair<string, JsonElement> item) => _items.Contains(item);

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns>True if the dictionary contains an element with the specified key; otherwise, false.</returns>
    public bool ContainsKey(string key) => _items.ContainsKey(key);

    /// <summary>
    /// Copies the elements of the dictionary to an array, starting at the specified array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the dictionary.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    public void CopyTo(KeyValuePair<string, JsonElement>[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An enumerator for the dictionary.</returns>
    public IEnumerator<KeyValuePair<string, JsonElement>> GetEnumerator() => _items.GetEnumerator();

    /// <summary>
    /// Removes the element with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>True if the element is successfully removed; otherwise, false.</returns>
    public bool Remove(string key)
    {
        if (_items.Remove(key))
        {
            SetKnownProperty(key, null);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes the first occurrence of a specific key/value pair from the dictionary.
    /// </summary>
    /// <param name="item">The key/value pair to remove.</param>
    /// <returns>True if the key/value pair was successfully removed; otherwise, false.</returns>
    public bool Remove(KeyValuePair<string, JsonElement> item)
    {
        if (_items.Remove(item.Key))
        {
            SetKnownProperty(item.Key, null);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if found; otherwise, the default value for the type.</param>
    /// <returns>True if the dictionary contains an element with the specified key; otherwise, false.</returns>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out JsonElement value) => _items.TryGetValue(key, out value);

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the dictionary.</returns>
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

    /// <summary>
    /// Sets a known property value based on the property key.
    /// Currently only handles the Type property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value to set.</param>
    private void SetKnownProperty(string key, JsonElement? value)
    {
        if (key == TypePropertyName)
        {
            Type = value?.GetString() ?? string.Empty;
        }
    }
}

/// <summary>
/// Represents window properties for a content item.
/// </summary>
public class ContentItemWindow
{
    /// <summary>
    /// Gets or sets the target name of the window.
    /// </summary>
    [JsonPropertyName("targetName")]
    public string? TargetName { get; set; }

    /// <summary>
    /// Gets or sets the window features.
    /// </summary>
    [JsonPropertyName("windowFeatures")]
    public string? WindowFeatures { get; set; }

    /// <summary>
    /// Gets or sets the width of the window.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the window.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }
}

/// <summary>
/// Represents thumbnail properties for a content item.
/// </summary>
public class ContentItemThumbnail
{
    /// <summary>
    /// Gets or sets the URL of the thumbnail.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the width of the thumbnail.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the thumbnail.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }
}

/// <summary>
/// Represents icon properties for a content item.
/// </summary>
public class ContentItemIcon
{
    /// <summary>
    /// Gets or sets the URL of the icon.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the width of the icon.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the icon.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }
}