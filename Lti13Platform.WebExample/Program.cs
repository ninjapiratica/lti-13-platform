using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.AssignmentGradeServices;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.DeepLinking;
using NP.Lti13Platform.DeepLinking.Models;
using NP.Lti13Platform.NameRoleProvisioningServices;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services
    .AddLti13PlatformCore(config =>
    {
        config.Issuer = "https://mytest.com";
    })
    .AddLti13PlatformDeepLinking(config =>
    {
        config.AddDefaultContentItemMapping();
    })
    .AddLti13PlatformAssignmentGradeServices()
    .AddLti13PlatformNameRoleProvisioningServices();

builder.Services.AddDevTunnelHttpContextAccessor();
builder.Services.AddTransient<IDeepLinkContentHandler, DeepLinkContentHandler>();
builder.Services.AddSingleton<ICoreDataService, DataService>();
builder.Services.AddSingleton<INameRoleProvisioningServicesDataService, DataService>();
builder.Services.AddSingleton<IDeepLinkingDataService, DataService>();
builder.Services.AddTransient<IAssignmentGradeServicesDataService, DataService>();

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

app.UseLti13PlatformCore()
    .UseLti13PlatformDeepLinking()
    .UseLti13PlatformAssignmentGradeServices()
    .UseLti13PlatformNameRoleProvisioningServices();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public class DataService : ICoreDataService, INameRoleProvisioningServicesDataService, IDeepLinkingDataService, IAssignmentGradeServicesDataService
{
    private static readonly CryptoProviderFactory CRYPTO_PROVIDER_FACTORY = new() { CacheSignatureProviders = false };

    Task<Tool?> ICoreDataService.GetToolAsync(string clientId)
    {
        return Task.FromResult<Tool?>(new Tool
        {
            Id = clientId,
            ClientId = clientId,
            OidcInitiationUrl = "https://saltire.lti.app/tool",
            LaunchUrl = "https://saltire.lti.app/tool",
            DeepLinkUrl = "https://saltire.lti.app/tool",
            Jwks = "https://saltire.lti.app/tool/jwks/1e49d5cbb9f93e9bb39a4c3cfcda929d",
            UserPermissions = new UserPermissions { FamilyName = true, Name = true, GivenName = true },
            CustomPermissions = new CustomPermissions() { UserUsername = true },
            ServiceScopes = [
                NP.Lti13Platform.AssignmentGradeServices.Lti13ServiceScopes.LineItem,
                NP.Lti13Platform.AssignmentGradeServices.Lti13ServiceScopes.LineItemReadOnly,
                NP.Lti13Platform.AssignmentGradeServices.Lti13ServiceScopes.ResultReadOnly,
                NP.Lti13Platform.AssignmentGradeServices.Lti13ServiceScopes.Score,
                NP.Lti13Platform.NameRoleProvisioningServices.Lti13ServiceScopes.MembershipReadOnly
            ]
        });
    }

    Task<Deployment?> ICoreDataService.GetDeploymentAsync(string deploymentId)
    {
        return Task.FromResult<Deployment?>(new Deployment { Id = deploymentId, ToolId = "asdfasdf" });
    }

    Task<Context?> ICoreDataService.GetContextAsync(string contextId)
    {
        return Task.FromResult<Context?>(new Context { Id = contextId, Label = "asdf_label", Title = "asdf_title", Types = [Lti13ContextTypes.CourseOffering] });
    }

    Task<User?> ICoreDataService.GetUserAsync(string userId)
    {
        return Task.FromResult<User?>(new User { Id = userId });
    }

    Task<Membership?> ICoreDataService.GetMembershipAsync(string contextId, string userId)
    {
        return Task.FromResult<Membership?>(null);
    }

    Task<IEnumerable<string>> ICoreDataService.GetMentoredUserIdsAsync(string contextId, string userId)
    {
        return Task.FromResult<IEnumerable<string>>([]);
    }

    Task<ResourceLink?> ICoreDataService.GetResourceLinkAsync(string resourceLinkId)
    {
        return Task.FromResult<ResourceLink?>(new ResourceLink
        {
            Id = new Guid().ToString(),
            DeploymentId = "",
            ContextId = ""
        });
    }

    Task<PartialList<LineItem>> ICoreDataService.GetLineItemsAsync(string deploymentId, string contextId, int pageIndex, int limit, string? resourceId = null, string? resourceLinkId = null, string? tag = null)
    {
        var totalItems = 23;
        return Task.FromResult(new PartialList<LineItem>
        {
            Items = Enumerable.Range(pageIndex * limit, Math.Max(0, Math.Min(limit, totalItems - pageIndex * limit))).Select(i => new LineItem
            {
                Id = new Guid().ToString(),
                DeploymentId = deploymentId,
                ContextId = contextId,
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

    // deeplinking && assignmentgradeservices
    public Task SaveLineItemAsync(LineItem lineItem)
    {
        throw new NotImplementedException();
    }

    async Task<Attempt?> ICoreDataService.GetAttemptAsync(string resourceLinkId, string userId)
    {
        return await Task.FromResult(new Attempt { ResourceLinkId = resourceLinkId, UserId = userId });
    }

    Task<PartialList<Grade>> IAssignmentGradeServicesDataService.GetGradesAsync(string lineItemId, int pageIndex, int limit, string? userId = null)
    {
        throw new NotImplementedException();
    }

    Task<Grade?> ICoreDataService.GetGradeAsync(string lineItemId, string userId)
    {
        throw new NotImplementedException();
    }

    Task IAssignmentGradeServicesDataService.SaveGradeAsync(Grade result)
    {
        throw new NotImplementedException();
    }

    private List<ServiceToken> _serviceTokens = [];
    Task<ServiceToken?> ICoreDataService.GetServiceTokenRequestAsync(string toolId, string serviceTokenId)
    {
        return Task.FromResult(_serviceTokens.FirstOrDefault(x => x.Id == serviceTokenId));
    }

    Task ICoreDataService.SaveServiceTokenRequestAsync(ServiceToken serviceToken)
    {
        _serviceTokens.Add(serviceToken);
        return Task.CompletedTask;
    }

    Task<IEnumerable<SecurityKey>> ICoreDataService.GetPublicKeysAsync()
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

    Task<SecurityKey> ICoreDataService.GetPrivateKeyAsync()
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



    Task<PartialList<Membership>> INameRoleProvisioningServicesDataService.GetMembershipsAsync(string deloymentId, string contextId, int pageIndex, int limit, string? role, string? resourceLinkId, DateTime? asOfDate = null)
    {
        return Task.FromResult(new PartialList<Membership>
        {
            Items = [
                new Membership { ContextId = contextId, Roles = [], Status = MembershipStatus.Active, UserId = "0" },
                new Membership { ContextId = contextId, Roles = [], Status = MembershipStatus.Inactive, UserId = "1" },
                new Membership { ContextId = contextId, Roles = [Lti13ContextRoles.Member], Status = MembershipStatus.Active, UserId = "2" },
            ],
            TotalItems = 3
        });
    }

    Task<IEnumerable<User>> INameRoleProvisioningServicesDataService.GetUsersAsync(IEnumerable<string> userIds, DateTime? asOfDate = null)
    {
        return Task.FromResult<IEnumerable<User>>([
            new User { Id = "0" },
            new User { Id = "1" },
            new User { Id = "2" },
        ]);
    }



    private List<ContentItem> _contentItems = [];
    Task<string> IDeepLinkingDataService.SaveContentItemAsync(string deploymentId, string? contextId, ContentItem contentItem)
    {
        _contentItems.Add(contentItem);
        return Task.FromResult(Guid.NewGuid().ToString());
    }



    Task<LineItem> IAssignmentGradeServicesDataService.GetLineItemAsync(string lineItemId)
    {
        throw new NotImplementedException();
    }

    Task IAssignmentGradeServicesDataService.DeleteLineItemAsync(string lineItemId)
    {
        throw new NotImplementedException();
    }
}

public class DeepLinkContentHandler : IDeepLinkContentHandler
{
    public Task<IResult> HandleAsync(DeepLinkResponse response)
    {
        return Task.FromResult(Results.Ok(response));
    }
}