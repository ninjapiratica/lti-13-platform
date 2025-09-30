using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.DeepLinking.Models;

/// <summary>
/// Represents a dictionary for content items with a specific tool Id and content item type.
/// </summary>
public class ContentItemDictionary() : IDictionary<(string? ClientId, string ContentItemType), Type>
{
    private readonly IDictionary<(string?, string), Type> _items = new Dictionary<(string?, string), Type>();

    /// <summary>
    /// Gets or sets the Type associated with the specified key.
    /// If the key is not found but has a non-null ClientId, attempts to find a Type with the same ContentItemType but null ClientId.
    /// If no match is found, returns DefaultContentItem type.
    /// </summary>
    /// <param name="key">The key containing ClientId and ContentItemType.</param>
    /// <returns>The Type associated with the specified key, or DefaultContentItem if not found.</returns>
    public Type this[(string? ClientId, string ContentItemType) key]
    {
        get => _items.TryGetValue(key, out Type? value) ? value : key.ClientId != null && _items.TryGetValue((null, key.ContentItemType), out value) ? value : typeof(DefaultContentItem);
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
/// Provides constants for different content item types as defined in the IMS Global LTI Deep Linking specification.
/// </summary>
public static class ContentItemType
{
    /// <summary>
    /// HTML Item: Allows display of HTML content. The content defined in the "html" property will be placed inside of 
    /// an iframe and presented to the user. The platform MAY allow the Tool to specify additional configuration, 
    /// such as the "frameheight" or "frameborder", though there is no guarantee that it will use this additional information.
    /// </summary>
    public static readonly string Html = "html";

    /// <summary>
    /// Link Item: The link type provides a simple URL link to a resource hosted on the internet.
    /// This content type might be used to provide a link to a paper, or reference material, a
    /// text book companion site, or any resource that is accessed by clicking on a link.
    /// </summary>
    public static readonly string Link = "link";

    /// <summary>
    /// LTI Resource Link Item: A link to an LTI resource, usually to be rendered within the same
    /// tool that provided the link, but when clicked, is a navigation from the platform to the tool.
    /// The object consists of a fully formed ContentItem plus these optional properties.
    /// </summary>
    public static readonly string LtiResourceLink = "ltiResourceLink";

    /// <summary>
    /// File Item: The file type provides a URL link to a file hosted on the internet.
    /// This content type might be used when uploading a new file to the platform, for example.
    /// </summary>
    public static readonly string File = "file";

    /// <summary>
    /// Image Item: The image type provides a URL link to an image resource hosted on the internet.
    /// This content type might be used when providing access to an image file, for example.
    /// </summary>
    public static readonly string Image = "image";
}

/// <summary>
/// Represents the base class for content items as defined in the IMS Global LTI Deep Linking specification.
/// Content Items are created by Tools and returned to Platforms to be stored and later presented to users.
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
    /// This is a required property that identifies which content-item type is being used.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }
}

/// <summary>
/// Represents a link content item as defined in the IMS Global LTI Deep Linking specification.
/// The link type provides a simple URL link to a resource hosted on the internet. 
/// This content type might be used to provide a link to a paper, or reference material, 
/// a text book companion site, or any resource that is accessed by clicking on a link.
/// </summary>
public class LinkContentItem : ContentItem
{
    /// <summary>
    /// Fully qualified URL of the resource. This link must be navigable to.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// String, plain text to use as the title or heading for content.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// String, plain text description of the content item intended to be displayed to all users who can access the item.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Fully qualified URL, height, and width of an icon image to be placed with the file.
    /// url: fully qualified URL to the image file.
    /// width: integer representing the width in pixels of the image.
    /// height: integer representing the height in pixels of the image.
    /// </summary>
    [JsonPropertyName("icon")]
    public ContentItemIcon? Icon { get; set; }

    /// <summary>
    /// Fully qualified URL, height, and width of a thumbnail image to be made a hyperlink.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public ContentItemThumbnail? Thumbnail { get; set; }

    /// <summary>
    /// The window property indicates how to open the resource in a new window/tab.
    /// </summary>
    [JsonPropertyName("window")]
    public ContentItemWindow? Window { get; set; }

    /// <summary>
    /// The iframe property indicates the resource can be embedded using an iframe.
    /// </summary>
    [JsonPropertyName("iframe")]
    public LinkIframe? Iframe { get; set; }

    /// <summary>
    /// The embed property has a single required property html that contains the HTML fragment to embed the resource directly inside HTML.
    /// </summary>
    [JsonPropertyName("embed")]
    public LinkEmbed? Embed { get; set; }

    /// <summary>
    /// Represents iframe properties for a link content item as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public class LinkIframe
    {
        /// <summary>
        /// Gets or sets the width of the iframe.
        /// The width of the iframe, in pixels.
        /// </summary>
        [JsonPropertyName("width")]
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the iframe.
        /// The height of the iframe, in pixels.
        /// </summary>
        [JsonPropertyName("height")]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets the source URL of the iframe.
        /// The URL to use as the src attribute of the iframe.
        /// </summary>
        [JsonPropertyName("src")]
        public string? Src { get; set; }
    }

    /// <summary>
    /// Represents embed properties for a link content item as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public class LinkEmbed
    {
        /// <summary>
        /// Gets or sets the HTML content for embed.
        /// The HTML to be embedded within the platform's page.
        /// </summary>
        [JsonPropertyName("html")]
        public required string Html { get; set; }
    }
}

/// <summary>
/// Represents an LTI resource link content item as defined in the IMS Global LTI Deep Linking specification.
/// A link to an LTI resource, usually to be rendered within the same tool that provided the link,
/// but when clicked, is a navigation from the platform to the tool.
/// </summary>
public class LtiResourceLinkContentItem : ContentItem
{
    /// <summary>
    /// Fully qualified url of the resource. If absent, the base LTI URL of the tool must be used for launch.
    /// If a platform receives a url then it MUST use this url as the target_link_uri in the LtiResourceLinkRequest payload.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// String, plain text to use as the title or heading for content.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// String, plain text description of the content item intended to be displayed to all users who can access the item.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Fully qualified URL, height, and width of an icon image to be placed with the file.
    /// </summary>
    [JsonPropertyName("icon")]
    public ContentItemIcon? Icon { get; set; }

    /// <summary>
    /// Fully qualified URL, height, and width of a thumbnail image to be made a hyperlink.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public ContentItemThumbnail? Thumbnail { get; set; }

    /// <summary>
    /// The window property indicates how to open the resource in a new window/tab.
    /// </summary>
    [JsonPropertyName("window")]
    public ContentItemWindow? Window { get; set; }

    /// <summary>
    /// The iframe property indicates the resource can be embedded using an iframe.
    /// </summary>
    [JsonPropertyName("iframe")]
    public LtiResourceLinkIframe? Iframe { get; set; }

    /// <summary>
    /// A map of key/value custom parameters. Those parameters MUST be included in the LtiResourceLinkRequest payload. Value may include substitution parameters as defined in the LTI Core Specification. Map values must be strings. Note that "empty-string" is a valid value (""); however, null is not a valid value.
    /// </summary>
    [JsonPropertyName("custom")]
    public IDictionary<string, string>? Custom { get; set; }

    /// <summary>
    /// A lineItem object that indicates this activity is expected to receive scores; the platform may automatically create a corresponding line item when the resource link is created, using the maximum score as the default maximum points.
    /// The resource_id, tag and scoreMaximum are defined in the LTI AGS specification. A line item created as a result of a Deep Linking interaction must be exposed in a subsequent line item service call, with the resourceLinkId of the associated resource link, as well as the resourceId and tag if present in the line item definition.
    /// label (optional): label for the line item. If not present, the title of the content item must be used.
    /// scoreMaximum (required): Positive decimal value indicating the maximum score possible for this activity.
    /// resourceId (optional): String, tool provided ID for the resource.
    /// tag (optional): String, additional information about the line item; may be used by the tool to identify line items attached to the same resource or resource link (example: grade, originality, participation).
    /// gradesReleased (optional): boolean to indicate if the platform should release the grades, e.g., to learners.
    /// </summary>
    [JsonPropertyName("lineItem")]
    public LtiResourceLinkLineItem? LineItem { get; set; }

    /// <summary>
    /// Indicates the initial start and end time this activity should be made available to learners.
    /// startDateTime (optional): ISO 8601 date and time when the link becomes accessible.
    /// endDateTime (optional): ISO 8601 date and time when the link stops being accessible.
    /// </summary>
    [JsonPropertyName("available")]
    public LtiResourceLinkAvailable? Available { get; set; }

    /// <summary>
    /// Indicates the initial start and end time submissions for this activity can be made by learners.
    /// startDateTime (optional): ISO 8601 Date and time when the link can start receiving submissions.
    /// endDateTime (optional): ISO 8601 Date and time when the link stops accepting submissions.
    /// </summary>
    [JsonPropertyName("submission")]
    public LtiResourceLinkSubmission? Submission { get; set; }

    /// <summary>
    /// Represents iframe properties for an LTI resource link content item as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    public class LtiResourceLinkIframe
    {
        /// <summary>
        /// Gets or sets the width of the iframe.
        /// The width of the iframe, in pixels.
        /// </summary>
        [JsonPropertyName("width")]
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the iframe.
        /// The height of the iframe, in pixels.
        /// </summary>
        [JsonPropertyName("height")]
        public int? Height { get; set; }
    }

    /// <summary>
    /// Represents line item properties for an LTI resource link content item as defined in the IMS Global LTI Deep Linking specification.
    /// A LineItem that should be associated with this LtiResourceLink.
    /// </summary>
    public class LtiResourceLinkLineItem
    {
        /// <summary>
        /// Gets or sets the label of the line item.
        /// Label for the line item. This is the heading that will be shown in the gradebook.
        /// </summary>
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the maximum score of the line item.
        /// The maximum score allowed for this line item. This is a required field that defines the 
        /// scaling of the score column.
        /// </summary>
        [JsonPropertyName("scoreMaximum")]
        public decimal ScoreMaximum { get; set; }

        /// <summary>
        /// Gets or sets the resource ID of the line item.
        /// An identifier for the resource linked to this line item. This can be used by tools
        /// to establish a unique reference to the resource across different contexts.
        /// </summary>
        [JsonPropertyName("resourceId")]
        public string? ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the tag of the line item.
        /// A tag used to mark the source of this line item. This allows line items to be grouped
        /// together in a gradebook, for example, to show all items from a specific tool.
        /// </summary>
        [JsonPropertyName("tag")]
        public string? Tag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grades are released.
        /// Flag indicating that the grades for this line item should be released to students immediately.
        /// </summary>
        [JsonPropertyName("gradesReleased")]
        public bool? GradesReleased { get; set; }
    }

    /// <summary>
    /// Represents availability properties for an LTI resource link content item as defined in the IMS Global LTI Deep Linking specification.
    /// Defines when this resource is available for student use.
    /// </summary>
    public class LtiResourceLinkAvailable
    {
        /// <summary>
        /// Gets or sets the start date and time of the availability period.
        /// The date and time when this resource becomes available to students.
        /// </summary>
        [JsonPropertyName("startDateTime")]
        public DateTimeOffset? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date and time of the availability period.
        /// The date and time when this resource is no longer available to students.
        /// </summary>
        [JsonPropertyName("endDateTime")]
        public DateTimeOffset? EndDateTime { get; set; }
    }

    /// <summary>
    /// Represents submission properties for an LTI resource link content item as defined in the IMS Global LTI Deep Linking specification.
    /// Defines when students can submit work for this resource.
    /// </summary>
    public class LtiResourceLinkSubmission
    {
        /// <summary>
        /// Gets or sets the start date and time of the submission period.
        /// The date and time when students can begin submitting work for this resource.
        /// </summary>
        [JsonPropertyName("startDateTime")]
        public DateTimeOffset? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date and time of the submission period.
        /// The date and time after which submissions will no longer be accepted for this resource.
        /// </summary>
        [JsonPropertyName("endDateTime")]
        public DateTimeOffset? EndDateTime { get; set; }
    }
}

/// <summary>
/// Represents a file content item as defined in the IMS Global LTI Deep Linking specification.
/// The file type provides a URL link to a file hosted on the internet.
/// This content type might be used when uploading a new file to the platform, for example.
/// </summary>
public class FileContentItem : ContentItem
{
    /// <summary>
    /// Fully qualified URL of the resource. This link may be short-lived or expire after 1st use.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// String, plain text to use as the title or heading for content.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// String, plain text description of the content item intended to be displayed to all users who can access the item.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Fully qualified URL, height, and width of an icon image to be placed with the file.
    /// </summary>
    [JsonPropertyName("icon")]
    public ContentItemIcon? Icon { get; set; }

    /// <summary>
    /// Fully qualified URL, height, and width of a thumbnail image to be made a hyperlink.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public ContentItemThumbnail? Thumbnail { get; set; }

    /// <summary>
    /// ISO 8601 Date and time. The URL will be available until this time. No guarantees after that. (e.g. 2014-03-05T12:34:56Z).
    /// </summary>
    [JsonPropertyName("expiresAt")]
    public DateTimeOffset? ExpiresAt { get; set; }
}

/// <summary>
/// Represents an HTML content item as defined in the IMS Global LTI Deep Linking specification.
/// HTML Item allows display of HTML content. The content defined in the "html" property
/// will be placed inside of an iframe and presented to the user.
/// </summary>
public class HtmlContentItem : ContentItem
{
    /// <summary>
    /// HTML fragment to be embedded. The platform is expected to sanitize it against cross-site scripting attacks.
    /// </summary>
    [JsonPropertyName("html")]
    public required string Html { get; set; }

    /// <summary>
    /// String, plain text to use as the title or heading for content.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// String, plain text description of the content item intended to be displayed to all users who can access the item.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

/// <summary>
/// Represents an image content item as defined in the IMS Global LTI Deep Linking specification.
/// The image type provides a URL link to an image resource hosted on the internet.
/// This content type might be used when providing access to an image file, for example.
/// </summary>
public class ImageContentItem : ContentItem
{
    /// <summary>
    /// Fully qualified URL of the image.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// String, plain text to use as the title or heading for content.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// String, plain text description of the content item intended to be displayed to all users who can access the item.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Fully qualified URL, height, and width of an icon image to be placed with the file.
    /// </summary>
    [JsonPropertyName("icon")]
    public ContentItemIcon? Icon { get; set; }

    /// <summary>
    /// Fully qualified URL, height, and width of a thumbnail image to be made a hyperlink.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public ContentItemThumbnail? Thumbnail { get; set; }

    /// <summary>
    /// Integer representing the width in pixels of the image.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    /// <summary>
    /// Integer representing the height in pixels of the image.
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
/// Represents window properties for a content item as defined in the IMS Global LTI Deep Linking specification.
/// This object defines how the content should be opened in a new window.
/// </summary>
public class ContentItemWindow
{
    /// <summary>
    /// String identifying the name of the window to open; this allows for a single window to be shared as the target of multiple links, preventing a proliferation of new windows/tabs.
    /// </summary>
    [JsonPropertyName("targetName")]
    public string? TargetName { get; set; }

    /// <summary>
    /// Comma-separated list of window features as per the [window.open() definition](https://developer.mozilla.org/en-US/docs/Web/API/Window/open).
    /// </summary>
    [JsonPropertyName("windowFeatures")]
    public string? WindowFeatures { get; set; }

    /// <summary>
    /// Integer representing the width in pixels of the new window
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    /// <summary>
    /// Integer representing the height in pixels of the new window
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }
}

/// <summary>
/// Represents thumbnail properties for a content item as defined in the IMS Global LTI Deep Linking specification.
/// A thumbnail image representation of the content.
/// </summary>
public class ContentItemThumbnail
{
    /// <summary>
    /// Fully qualified URL to the image file.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// Integer representing the width in pixels of the image.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    /// <summary>
    /// Integer representing the height in pixels of the image.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }
}

/// <summary>
/// Represents icon properties for a content item as defined in the IMS Global LTI Deep Linking specification.
/// An icon image representation of the content.
/// </summary>
public class ContentItemIcon
{
    /// <summary>
    /// Fully qualified URL to the image file.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// Integer representing the width in pixels of the image.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    /// <summary>
    /// Integer representing the height in pixels of the image.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }
}