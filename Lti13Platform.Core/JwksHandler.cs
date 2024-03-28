using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace NP.Lti13Platform
{
    public class JwksHandler(IDataService dataService)
    {
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

            return Results.Json(new
            {
                Keys = keySet.Keys.Select(k => (object)new
                {
                    k.Kty,
                    k.N,
                    k.E,
                    k.Alg,
                    k.Use,
                    k.Kid
                })
            });
        }
    }
}