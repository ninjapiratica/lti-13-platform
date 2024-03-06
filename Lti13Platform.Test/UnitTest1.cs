using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Test
{
    public class UnitTest1
    {
        private static readonly CryptoProviderFactory CRYPTO_PROVIDER_FACTORY = new() { CacheSignatureProviders = false };

        [Fact]
        public void Test1()
        {
            using var rsaProvider = RSA.Create();
            // Test key
            rsaProvider.ImportFromPem("-----BEGIN RSA PRIVATE KEY-----\r\nMIIEowIBAAKCAQEAl019/w1CjGtqDUn8Ozz63NEfSXK0WdawD3KUnOlDh4kGoRaR\r\nVIrfg/FLEraJewdSMAvTQnLzGDQFuPyI1FYmyFEHahHs/h8LJIyiy7pd9KhUsuP3\r\nKeYvd32bHGVz/u2gySOSRAZy88evvMGrFTtaqLeRUxW5BnyZut3tbpwqViXLizZq\r\nk5XfEVgbqx7V2QV9PVJf0a4u3exp72tAbQezF0q0Kipc13VfXYTZvexcrQuSWGcU\r\nzBNFOIKoETLcFK8wVbhTfo2Q0MBCMoEJogunTsZrrdEiCrF1XPIq7EFxM+JBWG1a\r\n1eDvxWnjQTqK0VVl2LfCMoRN/rD7rfDibQc0YQIDAQABAoIBAAMljnBGg1LOTRdX\r\nqZJF02XSR5dMdmnD6Ed595NH2qqv895XzM/4T2u8EfaiqztOzKvJIyynnVysgE33\r\nmpTn8ciKvt+63bXvSVkKP7yC9L9I3PIXgaVybxxKFXbCuWXc5VIpljop9CwTxBjl\r\n4jv/zwPhRXl34zA6WSwkv3JkdxDxkgTOxyoaHkbMre0CdP9Dl4AQiKyaf36IbXfh\r\nsIBOYvX5oP5Od5Y+Ug+s0aq2hjoglxImgZDyBlGCe+I+JDUPW+OvgsttFya8c3M9\r\nxJRW7BxiMoHqe+cd9EEl1nWXHqtamTPD54rmuqoVuwNM3KaLdq/dLZI/62mKlDIF\r\nHSa/TwECgYEA4m5Ivh3HjLjU3C1lG0E3WMbtxqbB5mG8n+Yogytj7POSEz0ul2JF\r\nUm2cvFnNUdWe6OSRG73QkRntJIofQAAOWSlOEFFDM1DdLRg9kLMStEo2JoNPjDAM\r\n+TSk3R1zkJNeDzNtkZO1f+WejTVcupN3qEsuTF+g+o8rAE8P73YuQdECgYEAqw+h\r\nnISXOBD2DudllXTyn7zRxXe1m98u2ZE2pZd+uRypO7PMpfVMiscsCrJoHbBkXE6J\r\n9Wb9KQgVKHxdtuaxkOuKqk1t8VjlHQyC/VO88yXwFE+yQ9nrCTEcu2o2Eb9Ey7a6\r\nbCLApgBmpU0QWLYM1wj18GP7PiATpfzHL3f7XZECgYAVJswwxkNfx9xKfQsW0q7C\r\n4kJP7j/qr3KZVTyvlBwPhGk+1tZFWe6z1n1vssvVOylPBBryBnc3Nr7KTQTCS78L\r\nYSpjp9OpNYKTtdH6dF/o643HZzjFFbAAj4RfC2NCPCHrNZikorGvstluw29YFnJ1\r\nDCDVDZHSFhGkQ75vVhDYIQKBgFlxG+R144eaPr3+ObxS4MWq+dgRRrEQmjOCXRtq\r\nQgVSOh6QXZHs16+8goe5Tv0vDNrC6hmZVweMRVvc4zdOGkwXDHMNd035WBq/PwJs\r\nNWDBVm2YWjJmECHHPymzWEAhTTxi98iwxyBFF2aZC9IGpmINOmMONAEAzqU8rX1h\r\nc9oxAoGBAIzUwcmOWndLyL9swO8TIqRODE4NuHjwmc6SyrGMAaSZZvPnP8AOO7AE\r\nXFhNjuWFaQn/zTwkNKPzWxWyuDQp/0Sk3pGV6XteV0ShuJrOSAFSR0h9GdTMH6ZK\r\nv+8h7srC/8nHogRvbhYbA7ffAvvVP0wkJRz0jaMkVeY9cMZStJzT\r\n-----END RSA PRIVATE KEY-----\r\n");

            var x = JsonSerializer.Serialize(new TestClass { });
            var y = JsonSerializer.Serialize(new TestClass { Val = "not null" });

            var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Issuer = "config.Issuer",
                Audience = "request.Client_Id",
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsaProvider) { CryptoProviderFactory = CRYPTO_PROVIDER_FACTORY }, SecurityAlgorithms.RsaSha256),
                Claims = new Dictionary<string, object> { { "user", JsonSerializer.SerializeToElement(new TestClass()) }, { "user2", JsonSerializer.SerializeToElement(new TestClass { Val = "also not null" }) } }
            });

            Debug.WriteLine(x);
            Debug.WriteLine(y);
            Debug.WriteLine(token);
        }

        public class TestClass
        {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Val { get; set; }
        }
    }
}