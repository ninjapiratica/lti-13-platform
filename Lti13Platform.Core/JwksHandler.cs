using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace NP.Lti13Platform
{
    public class JwksHandler(IDataService dataService)
    {
        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
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

        public async Task<IResult> HandleAsync()
        {
            var keys = await dataService.GetPublicKeysAsync();
            var keySet = new JsonWebKeySet();

            foreach (var key in keys)
            {
                var jwk = JsonWebKeyConverter.ConvertFromSecurityKey(key);
                jwk.Use = JsonWebKeyUseNames.Sig;
                jwk.Alg = SecurityAlgorithms.RsaSha256;
                keySet.Keys.Add(jwk);
            }

            return Results.Json(keySet, jsonOptions);
        }
    }
}