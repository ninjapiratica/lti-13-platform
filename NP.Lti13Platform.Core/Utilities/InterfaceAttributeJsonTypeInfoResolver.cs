using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace NP.Lti13Platform.Core.Utilities;

/// <summary>
/// A custom <see cref="DefaultJsonTypeInfoResolver"/> that applies JSON serialization attributes
/// from implemented interfaces to the concrete type's property metadata.
/// </summary>
/// <remarks>
/// This resolver ensures that attributes such as <see cref="JsonPropertyNameAttribute"/> and
/// <see cref="JsonIgnoreAttribute"/> defined on interface properties are honored during serialization,
/// even when the concrete type inherits the property from a base class without those attributes.
/// </remarks>
internal sealed class InterfaceAttributeJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var typeInfo = base.GetTypeInfo(type, options);

        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return typeInfo;

        var interfaces = type.GetInterfaces();
        if (interfaces.Length == 0)
            return typeInfo;

        foreach (var iface in interfaces)
        {
            foreach (var ifaceProp in iface.GetProperties())
            {
                var jsonPropName = ifaceProp.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false)
                    .OfType<JsonPropertyNameAttribute>()
                    .FirstOrDefault();

                var jsonIgnore = ifaceProp.GetCustomAttributes(typeof(JsonIgnoreAttribute), false)
                    .OfType<JsonIgnoreAttribute>()
                    .FirstOrDefault();

                if (jsonPropName is null && jsonIgnore is null)
                    continue;

                // Find matching property in typeInfo by the CLR property name
                var targetProp = FindPropertyInfo(typeInfo, ifaceProp.Name);
                if (targetProp is null)
                    continue;

                if (jsonPropName is not null)
                {
                    targetProp.Name = jsonPropName.Name;
                }

                if (jsonIgnore is not null)
                {
                    targetProp.ShouldSerialize = (_, _) => false;
                }
            }
        }

        return typeInfo;
    }

    private static JsonPropertyInfo? FindPropertyInfo(JsonTypeInfo typeInfo, string clrPropertyName)
    {
        foreach (var prop in typeInfo.Properties)
        {
            var name = prop.AttributeProvider is MemberInfo member ? member.Name : null;

            // Match by the original CLR property name (before any naming policy)
            if (string.Equals(name, clrPropertyName, StringComparison.OrdinalIgnoreCase))
            {
                return prop;
            }
        }

        return null;
    }
}