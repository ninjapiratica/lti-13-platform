using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace NP.Lti13Platform
{
    public class JwksHandler
    {
        public Task<IResult> HandleAsync()
        {
            using var rsaProvider = RSA.Create();
            var key = "-----BEGIN PUBLIC KEY-----\r\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArSpqDfXwj0ZYB0yQTL96\r\n7oaTiOI35kaudwkF2NFRPKkxF43oqtlzdX7TuTzvhVmIW8iY9ZqDVcH9av+MfA5D\r\n3YYMPnRws2+b2DE16cN+qKqonuMtaj9RERLYrC2Gz2fDB612L8TZi7KV/AFESeVt\r\n3rAGGSeXc8PLRvPz/WU0o4JGnsbqaY2morgcHssHWurAWlrNHM4cYnz5ku9BM2Os\r\nT3vTKjQCW9pcEfGtBuPPOhVUK8799GOZTeIsU4Uvjv+l+FoINJwRqeaoisA5nh2M\r\nxbP2CyCiAW9b0oWFRCoJwDz6HKUGVZWsclLmLosjrKK1nxHmWyy5jxxak7YNCyhf\r\nmQIDAQAB\r\n-----END PUBLIC KEY-----\r\n";
            rsaProvider.ImportFromPem(key);
            var securityKey = new RsaSecurityKey(rsaProvider.ExportParameters(false))
            {
                KeyId = "asdf"
            };

            var jwk = JsonWebKeyConverter.ConvertFromSecurityKey(securityKey);

            var keySet = new JsonWebKeySet();
            keySet.Keys.Add(jwk);

            return Task.FromResult(Results.Ok(keySet));
        }
    }
}