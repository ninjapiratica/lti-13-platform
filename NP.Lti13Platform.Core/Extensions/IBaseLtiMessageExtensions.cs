using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.MessageClaims;
using System.Text.Json;

namespace NP.Lti13Platform.Core.Extensions;

/// <summary>
/// Provides extension methods for working with ILtiMessage instances and their extensions.
/// </summary>
/// <remarks>This static class contains helper methods that enable combining or manipulating LTI message objects
/// with additional extension data. These methods are intended to simplify the process of augmenting LTI messages with
/// custom or standard extension fields in a type-safe manner.</remarks>
public static class IBaseLtiMessageExtensions
{
    /// <summary>
    /// Creates a dictionary representation of the specified LTI message, extended with additional properties from one or more extension objects.
    /// </summary>
    /// <remarks>This method serializes both the message and each extension object to JSON, then merges their properties into a single dictionary.
    /// The resulting dictionary can be used for further processing or serialization.
    /// The method does not modify the original message or extension objects.</remarks>
    /// <typeparam name="T">The type of the LTI message. Must implement the IBaseLtiMessageExtensions interface.</typeparam>
    /// <param name="message">The LTI message to convert to a dictionary and extend with additional properties.</param>
    /// <param name="extensions">An array of extension objects whose properties will be added to the resulting dictionary.
    /// If multiple extensions define the same property, the last one takes precedence.</param>
    /// <returns>A dictionary containing the combined properties of the original message and all specified extensions.
    /// Properties from extensions override those in the message or previous extensions if keys conflict.</returns>
    public static IDictionary<string, JsonElement> Extend<T>(this T message, IEnumerable<object> extensions)
        where T : IBaseLtiMessage
    {
        static Dictionary<string, JsonElement> ToDict(object obj)
        {
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(obj, JsonSerializerLtiMessageOptions.LTI_MESSAGE_JSON_SERIALIZER_OPTIONS));
            return doc.RootElement
                .EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.Clone());
        }

        var result = ToDict(message);

        foreach (var obj in extensions)
        {
            foreach (var kv in ToDict(obj))
            {
                result[kv.Key] = kv.Value;
            }
        }

        return result;
    }
}
