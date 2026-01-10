using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace NP.Lti13Platform.Core.Constants;

/// <summary>
/// Provides a preconfigured set of JSON serialization options for LTI 1.3 message processing.
/// </summary>
/// <remarks>Use this property when serializing or deserializing LTI 1.3 messages to ensure consistent JSON formatting
/// and compatibility with LTI 1.3 specifications. The options include camel case property naming, omission of null values,
/// and support for serializing enums as strings. Collections are only serialized if they contain elements, which helps
/// reduce payload size and improves interoperability with LTI 1.3 consumers.</remarks>
public static class JsonSerializerMessageOptions
{
    internal static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                (typeInfo) =>
                {
                    foreach(var prop in typeInfo.Properties.Where(p => p.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(p.PropertyType)))
                    {
                        prop.ShouldSerialize = (obj, val) => val is IEnumerable e && e.GetEnumerator().MoveNext();
                    }
                }
            }
        }
    };

    /// <summary>
    /// Provides preconfigured JSON serialization options for LTI 1.3 message processing.
    /// </summary>
    /// <remarks>Use this instance when serializing or deserializing LTI 1.3 messages to ensure consistent
    /// handling of JSON formatting and compatibility with LTI 1.3 specifications. The options are based on the default
    /// settings defined in <see cref="JSON_SERIALIZER_OPTIONS"/>.</remarks>
    public static readonly JsonSerializerOptions LTI_13_MESSAGE_JSON_SERIALIZER_OPTIONS = new(JSON_SERIALIZER_OPTIONS);
}
