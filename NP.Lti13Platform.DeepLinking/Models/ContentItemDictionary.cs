using NP.Lti13Platform.Core.Models;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace NP.Lti13Platform.DeepLinking.Models;

/// <summary>
/// Represents a dictionary for content items with a specific tool Id and content item type.
/// </summary>
public class ContentItemDictionary() : IDictionary<(ClientId? ClientId, string ContentItemType), Type>
{
    private readonly IDictionary<(ClientId?, string), Type> _items = new Dictionary<(ClientId?, string), Type>();

    /// <summary>
    /// Gets or sets the Type associated with the specified key.
    /// If the key is not found but has a non-null ClientId, attempts to find a Type with the same ContentItemType but null ClientId.
    /// If no match is found, returns DefaultContentItem type.
    /// </summary>
    /// <param name="key">The key containing ClientId and ContentItemType.</param>
    /// <returns>The Type associated with the specified key, or DefaultContentItem if not found.</returns>
    public Type this[(ClientId? ClientId, string ContentItemType) key]
    {
        get => _items.TryGetValue(key, out Type? value) ? value : key.ClientId != null && _items.TryGetValue((null, key.ContentItemType), out value) ? value : typeof(DefaultContentItem);
        set => _items[key] = value;
    }

    /// <summary>
    /// Gets a collection containing the keys in the dictionary.
    /// </summary>
    public ICollection<(ClientId?, string)> Keys => _items.Keys;

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
    public void Add((ClientId?, string) key, Type value) => _items[key] = value;

    /// <summary>
    /// Adds the specified key/value pair to the dictionary.
    /// </summary>
    /// <param name="item">The key/value pair to add.</param>
    public void Add(KeyValuePair<(ClientId?, string), Type> item) => _items[item.Key] = item.Value;

    /// <summary>
    /// Removes all keys and values from the dictionary.
    /// </summary>
    public void Clear() => _items.Clear();

    /// <summary>
    /// Determines whether the dictionary contains a specific key/value pair.
    /// </summary>
    /// <param name="item">The key/value pair to locate in the dictionary.</param>
    /// <returns>True if the key/value pair is found in the dictionary; otherwise, false.</returns>
    public bool Contains(KeyValuePair<(ClientId?, string), Type> item) => _items.Contains(item);

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns>True if the dictionary contains an element with the specified key; otherwise, false.</returns>
    public bool ContainsKey((ClientId?, string) key) => _items.ContainsKey(key);

    /// <summary>
    /// Copies the elements of the dictionary to an array, starting at the specified array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the dictionary.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    public void CopyTo(KeyValuePair<(ClientId?, string), Type>[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An enumerator for the dictionary.</returns>
    public IEnumerator<KeyValuePair<(ClientId?, string), Type>> GetEnumerator() => _items.GetEnumerator();

    /// <summary>
    /// Removes the element with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>True if the element is successfully removed; otherwise, false.</returns>
    public bool Remove((ClientId?, string) key) => _items.Remove(key);

    /// <summary>
    /// Removes the first occurrence of a specific key/value pair from the dictionary.
    /// </summary>
    /// <param name="item">The key/value pair to remove.</param>
    /// <returns>True if the key/value pair was successfully removed; otherwise, false.</returns>
    public bool Remove(KeyValuePair<(ClientId?, string), Type> item) => _items.Remove(item.Key);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// Always returns true and sets the value to this[key], which may be DefaultContentItem if not found.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found;
    /// otherwise, the default value for the type of the value parameter.</param>
    /// <returns>Always returns true.</returns>
    public bool TryGetValue((ClientId?, string) key, [MaybeNullWhen(false)] out Type value)
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
