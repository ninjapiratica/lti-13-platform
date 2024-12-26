using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using NP.Lti13Platform.Core.Populators;

namespace NP.Lti13Platform.Core;

internal class LtiMessageTypeResolver : DefaultJsonTypeInfoResolver
{
    private static readonly HashSet<Type> derivedTypes = [];

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        var baseType = typeof(LtiMessage);
        if (jsonTypeInfo.Type == baseType)
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                IgnoreUnrecognizedTypeDiscriminators = true,
            };

            foreach (var derivedType in derivedTypes)
            {
                jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType));
            }
        }

        return jsonTypeInfo;
    }

    public static void AddDerivedType(Type type)
    {
        derivedTypes.Add(type);
    }
}
