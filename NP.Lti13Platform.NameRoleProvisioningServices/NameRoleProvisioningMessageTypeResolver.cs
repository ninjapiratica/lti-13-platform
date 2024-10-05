using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    internal class NameRoleProvisioningMessageTypeResolver : DefaultJsonTypeInfoResolver
    {
        private static readonly HashSet<Type> derivedTypes = [];

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var jsonTypeInfo = base.GetTypeInfo(type, options);

            var baseType = typeof(NameRoleProvisioningMessage);
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
}
