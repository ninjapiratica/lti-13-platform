using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform;
using NP.Lti13Platform.Models;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddLti13Platform(config =>
{
    config.Issuer = "https://mytest.com";
    config.DeepLink.AcceptMultiple = true;
    config.DeepLink.AcceptLineItem = true;
    config.DeepLink.AutoCreate = true;
    config.TokenAudience = "https://05e4-2601-1c1-8400-cd97-00-1005.ngrok-free.app/lti13/token";
});
builder.Services.AddSingleton<IDataService, DataService>();
builder.Services.AddTransient<IDeepLinkContentHandler, DeepLinkContentHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseLti13Platform();

app.Map("/test/{x?}", (int? x) => x);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public class DataService : IDataService
{
    private static readonly CryptoProviderFactory CRYPTO_PROVIDER_FACTORY = new() { CacheSignatureProviders = false };

    public Task<Tool?> GetToolAsync(string clientId)
    {
        return Task.FromResult<Tool?>(new Tool { ClientId = clientId, OidcInitiationUrl = "https://saltire.lti.app/tool", LaunchUrl = "https://saltire.lti.app/tool", DeepLinkUrl = "https://saltire.lti.app/tool", Jwks = "https://saltire.lti.app/tool/jwks/1e49d5cbb9f93e9bb39a4c3cfcda929d", UserPermissions = new UserPermissions { FamilyName = true, Name = true, GivenName = true } });
    }

    public Task<Context?> GetContextAsync(string contextId)
    {
        return Task.FromResult<Context?>(new Context { Id = contextId, DeploymentId = "asdf", Label = "asdf_label", Title = "asdf_title", Types = [Lti13ContextTypes.CourseOffering] });
    }

    public Task<Deployment?> GetDeploymentAsync(string deploymentId)
    {
        return Task.FromResult<Deployment?>(new Deployment { Id = deploymentId, ClientId = "asdf" });
    }

    public Task<IEnumerable<string>> GetMentoredUserIdsAsync(string userId, Context? context)
    {
        return Task.FromResult<IEnumerable<string>>([]);
    }

    public Task<LtiResourceLinkContentItem?> GetResourceLinkAsync(string resourceLinkId)
    {
        var contentItem = _contentItems.Count > 0 ? _contentItems[0] as LtiResourceLinkContentItem : null;
        return Task.FromResult<LtiResourceLinkContentItem?>(contentItem);
    }

    public Task<IEnumerable<string>> GetRolesAsync(string userId, Context? context)
    {
        return Task.FromResult<IEnumerable<string>>([]);
    }

    public Task<User?> GetUserAsync(string userId)
    {
        return Task.FromResult<User?>(new User { Id = userId, Name = "name", FamilyName = "familyname", GivenName = "givenname", Address = new Address { Id = "addressid", Country = "country" }, Email = "email@email.com" });
    }

    private List<ContentItem> _contentItems = [];
    public Task SaveContentItemsAsync(IEnumerable<ContentItem> contentItems)
    {
        _contentItems.AddRange(contentItems);
        return Task.CompletedTask;
    }

    private List<ServiceToken> _serviceTokens = [];
    public Task<ServiceToken?> GetServiceTokenRequestAsync(string id)
    {
        return Task.FromResult(_serviceTokens.FirstOrDefault(x => x.Id == id));
    }

    public Task SaveServiceTokenRequestAsync(ServiceToken serviceToken)
    {
        _serviceTokens.Add(serviceToken);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<SecurityKey>> GetPublicKeysAsync()
    {
        var rsaProvider = RSA.Create();
        var key = "-----BEGIN PUBLIC KEY-----\r\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA6S7asUuzq5Q/3U9rbs+P\r\nkDVIdjgmtgWreG5qWPsC9xXZKiMV1AiV9LXyqQsAYpCqEDM3XbfmZqGb48yLhb/X\r\nqZaKgSYaC/h2DjM7lgrIQAp9902Rr8fUmLN2ivr5tnLxUUOnMOc2SQtr9dgzTONY\r\nW5Zu3PwyvAWk5D6ueIUhLtYzpcB+etoNdL3Ir2746KIy/VUsDwAM7dhrqSK8U2xF\r\nCGlau4ikOTtvzDownAMHMrfE7q1B6WZQDAQlBmxRQsyKln5DIsKv6xauNsHRgBAK\r\nctUxZG8M4QJIx3S6Aughd3RZC4Ca5Ae9fd8L8mlNYBCrQhOZ7dS0f4at4arlLcaj\r\ntwIDAQAB\r\n-----END PUBLIC KEY-----";
        rsaProvider.ImportFromPem(key);
        var securityKey = new RsaSecurityKey(rsaProvider)
        {
            KeyId = "asdf",
            CryptoProviderFactory = CRYPTO_PROVIDER_FACTORY
        };

        return Task.FromResult<IEnumerable<SecurityKey>>([securityKey]);
    }

    public Task<SecurityKey> GetPrivateKeyAsync()
    {
        var rsaProvider = RSA.Create();
        var key = "-----BEGIN PRIVATE KEY-----\r\nMIIEwAIBADANBgkqhkiG9w0BAQEFAASCBKowggSmAgEAAoIBAQDpLtqxS7OrlD/d\r\nT2tuz4+QNUh2OCa2Bat4bmpY+wL3FdkqIxXUCJX0tfKpCwBikKoQMzddt+ZmoZvj\r\nzIuFv9eploqBJhoL+HYOMzuWCshACn33TZGvx9SYs3aK+vm2cvFRQ6cw5zZJC2v1\r\n2DNM41hblm7c/DK8BaTkPq54hSEu1jOlwH562g10vcivbvjoojL9VSwPAAzt2Gup\r\nIrxTbEUIaVq7iKQ5O2/MOjCcAwcyt8TurUHpZlAMBCUGbFFCzIqWfkMiwq/rFq42\r\nwdGAEApy1TFkbwzhAkjHdLoC6CF3dFkLgJrkB7193wvyaU1gEKtCE5nt1LR/hq3h\r\nquUtxqO3AgMBAAECggEBANX6C+7EA/TADrbcCT7fMuNnMb5iGovPuiDCWc6bUIZC\r\nQ0yac45l7o1nZWzfzpOkIprJFNZoSgIF7NJmQeYTPCjAHwsSVraDYnn3Y4d1D3tM\r\n5XjJcpX2bs1NactxMTLOWUl0JnkGwtbWp1Qq+DBnMw6ghc09lKTbHQvhxSKNL/0U\r\nC+YmCYT5ODmxzLBwkzN5RhxQZNqol/4LYVdji9bS7N/UITw5E6LGDOo/hZHWqJsE\r\nfgrJTPsuCyrYlwrNkgmV2KpRrGz5MpcRM7XHgnqVym+HyD/r9E7MEFdTLEaiiHcm\r\nIsh1usJDEJMFIWkF+rnEoJkQHbqiKlQBcoqSbCmoMWECgYEA/4379mMPF0JJ/EER\r\n4VH7/ZYxjdyphenx2VYCWY/uzT0KbCWQF8KXckuoFrHAIP3EuFn6JNoIbja0NbhI\r\nHGrU29BZkATG8h/xjFy/zPBauxTQmM+yS2T37XtMoXNZNS/ubz2lJXMOapQQiXVR\r\nl/tzzpyWaCe9j0NT7DAU0ZFmDbECgYEA6ZbjkcOs2jwHsOwwfamFm4VpUFxYtED7\r\n9vKzq5d7+Ii1kPKHj5fDnYkZd+mNwNZ02O6OGxh40EDML+i6nOABPg/FmXeVCya9\r\nVump2Yqr2fAK3xm6QY5KxAjWWq2kVqmdRmICSL2Z9rBzpXmD5o06y9viOwd2bhBo\r\n0wB02416GecCgYEA+S/ZoEa3UFazDeXlKXBn5r2tVEb2hj24NdRINkzC7h23K/z0\r\npDZ6tlhPbtGkJodMavZRk92GmvF8h2VJ62vAYxamPmhqFW5Qei12WL+FuSZywI7F\r\nq/6oQkkYT9XKBrLWLGJPxlSKmiIGfgKHrUrjgXPutWEK1ccw7f10T2UXvgECgYEA\r\nnXqLa58G7o4gBUgGnQFnwOSdjn7jkoppFCClvp4/BtxrxA+uEsGXMKLYV75OQd6T\r\nIhkaFuxVrtiwj/APt2lRjRym9ALpqX3xkiGvz6ismR46xhQbPM0IXMc0dCeyrnZl\r\nQKkcrxucK/Lj1IBqy0kVhZB1IaSzVBqeAPrCza3AzqsCgYEAvSiEjDvGLIlqoSvK\r\nMHEVe8PBGOZYLcAdq4YiOIBgddoYyRsq5bzHtTQFgYQVK99Cnxo+PQAvzGb+dpjN\r\n/LIEAS2LuuWHGtOrZlwef8ZpCQgrtmp/phXfVi6llcZx4mMm7zYmGhh2AsA9yEQc\r\nacgc4kgDThAjD7VlXad9UHpNMO8=\r\n-----END PRIVATE KEY-----";
        rsaProvider.ImportFromPem(key);
        var securityKey = new RsaSecurityKey(rsaProvider)
        {
            KeyId = "asdf",
            CryptoProviderFactory = CRYPTO_PROVIDER_FACTORY
        };

        return Task.FromResult<SecurityKey>(securityKey);
    }

    public Task<PartialList<LineItem>> GetLineItemsAsync(string contextId, int pageIndex, int limit, string? resourceId, string? resourceLinkId, string? tag)
    {
        var totalItems = 23;
        return Task.FromResult(new PartialList<LineItem>
        {
            Items = Enumerable.Range(pageIndex * limit, Math.Max(0, Math.Min(limit, totalItems - pageIndex * limit))).Select(i => new LineItem
            {
                Id = new Guid().ToString(),
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.UtcNow,
                Label = "label " + i,
                ResourceId = "resource id " + i,
                ResourceLinkId = new Guid().ToString(),
                ScoreMaximum = 1.1m * i,
                Tag = "tag " + i
            }),
            TotalItems = totalItems
        });
    }

    public Task SaveLineItemAsync(LineItem lineItem)
    {
        throw new NotImplementedException();
    }

    public Task<LineItem?> GetLineItemAsync(string lineItemId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteLineItemAsync(string lineItemId)
    {
        throw new NotImplementedException();
    }

    public Task<PartialList<Result>> GetLineItemResultsAsync(string contextId, string lineItemId, int pageIndex, int limit, string? userId)
    {
        throw new NotImplementedException();
    }

    public Task SaveLineItemResultAsync(Result result)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetContentItemAsync<T>(string contentItemId) where T : ContentItem
    {
        return Task.FromResult(_contentItems.FirstOrDefault(c => c.Id == contentItemId) as T);
    }
}

public class DeepLinkContentHandler : IDeepLinkContentHandler
{
    public Task<IResult> HandleAsync(DeepLinkResponse response)
    {
        return Task.FromResult(Results.Ok(response));
    }
}