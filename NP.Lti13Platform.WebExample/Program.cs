using Microsoft.Extensions.DependencyInjection.Extensions;
using NP.Lti13Platform;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.DeepLinking.Configs;
using NP.Lti13Platform.WebExample;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services
    .AddLti13Platform()
    .WithLti13DataService<DataService>();

builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = (description) => description.GroupName == null || description.GroupName == "v1";
});
builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = (description) => description.GroupName == OpenApi.GroupName;
    options.AddDocumentTransformer<OpenApi.DocumentTransformer>();
    options.AddOperationTransformer<OpenApi.OperationTransformer>();
});
builder.Services.AddLti13OpenApi("lti");

builder.Services.RemoveAll<IHttpContextAccessor>();
builder.Services.AddSingleton<IHttpContextAccessor, DevTunnelHttpContextAccessor>();

builder.Services.Configure<DeepLinkingConfig>(x =>
{
    x.AddDefaultContentItemMapping();
});

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

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Public API");
    options.SwaggerEndpoint("/openapi/v2.json", "LTI 1.3 API Manual");
    options.SwaggerEndpoint("/openapi/lti.json", "LTI 1.3 API Automatic");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("", () => { });

app.Run();

namespace NP.Lti13Platform.WebExample
{
    using Microsoft.IdentityModel.Tokens;
    using NP.Lti13Platform.AssignmentGradeServices.Services;
    using NP.Lti13Platform.Core.Constants;
    using NP.Lti13Platform.Core.Models;
    using NP.Lti13Platform.Core.Services;
    using NP.Lti13Platform.DeepLinking.Models;
    using NP.Lti13Platform.DeepLinking.Services;
    using NP.Lti13Platform.NameRoleProvisioningServices.Services;
    using System.Security.Cryptography;

    /// <summary>
    /// A sample implementation of the <see cref="ILti13DataService"/> for demonstration purposes.
    /// </summary>
    public class DataService : ILti13DataService
    {
        private static readonly CryptoProviderFactory CRYPTO_PROVIDER_FACTORY = new() { CacheSignatureProviders = false };

        /// <summary>
        /// In-memory list of attempts.
        /// </summary>
        public static readonly List<Attempt> Attempts = [];
        /// <summary>
        /// In-memory list of contexts.
        /// </summary>
        public static readonly List<Context> Contexts = [];
        /// <summary>
        /// In-memory list of deployments.
        /// </summary>
        public static readonly List<Deployment> Deployments = [];
        /// <summary>
        /// In-memory list of grades.
        /// </summary>
        public static readonly List<Grade> Grades = [];
        /// <summary>
        /// In-memory list of line items.
        /// </summary>
        public static readonly List<LineItem> LineItems = [];
        /// <summary>
        /// In-memory list of memberships.
        /// </summary>
        public static readonly List<Membership> Memberships = [];
        /// <summary>
        /// In-memory list of resource links.
        /// </summary>
        public static readonly List<ResourceLink> ResourceLinks = [];
        /// <summary>
        /// In-memory list of service tokens.
        /// </summary>
        public static readonly List<ServiceToken> ServiceTokens = [];
        /// <summary>
        /// In-memory list of tools.
        /// </summary>
        public static readonly List<Tool> Tools = [];
        /// <summary>
        /// In-memory list of users.
        /// </summary>
        public static readonly List<User> Users = [];

        static DataService()
        {
            Tools.Add(new Tool
            {
                ClientId = "clientId",
                OidcInitiationUrl = new Uri("https://saltire.lti.app/tool"),
                LaunchUrl = new Uri("https://saltire.lti.app/tool"),
                DeepLinkUrl = new Uri("https://saltire.lti.app/tool"),
                Jwks = "https://saltire.lti.app/tool/jwks/sb86616b3bd1071dfbd24ff3b251e2570",
                ServiceScopes =
                [
                    AssignmentGradeServices.ServiceScopes.LineItem,
                    AssignmentGradeServices.ServiceScopes.LineItemReadOnly,
                    AssignmentGradeServices.ServiceScopes.ResultReadOnly,
                    AssignmentGradeServices.ServiceScopes.Score,
                    NameRoleProvisioningServices.Lti13ServiceScopes.MembershipReadOnly
                ]
            });

            Deployments.Add(new Deployment
            {
                Id = "deploymentId",
                ClientId = "toolId"
            });

            Contexts.Add(new Context
            {
                Id = "contextId",
                Label = "asdf_label",
                Title = "asdf_title",
                Types = [Lti13ContextTypes.CourseOffering]
            });

            Users.Add(new User
            {
                Id = "userId"
            });

            Memberships.Add(new Membership
            {
                ContextId = "contextId",
                Roles = [],
                Status = MembershipStatus.Active,
                UserId = "userId",
                MentoredUserIds = []
            });
        }

        Task<Tool?> ILti13CoreDataService.GetToolAsync(string clientId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Tools.SingleOrDefault(t => t.ClientId == clientId));
        }

        Task<Deployment?> ILti13CoreDataService.GetDeploymentAsync(string deploymentId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Deployments.SingleOrDefault(d => d.Id == deploymentId));
        }

        Task<Context?> ILti13CoreDataService.GetContextAsync(string contextId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Contexts.SingleOrDefault(c => c.Id == contextId));
        }

        Task<User?> ILti13CoreDataService.GetUserAsync(string userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Users.SingleOrDefault(u => u.Id == userId));
        }

        Task<Membership?> ILti13CoreDataService.GetMembershipAsync(string contextId, string userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Memberships.SingleOrDefault(m => m.ContextId == contextId && m.UserId == userId));
        }

        Task<ResourceLink?> ILti13CoreDataService.GetResourceLinkAsync(string resourceLinkId, CancellationToken cancellationToken)
        {
            return Task.FromResult(ResourceLinks.SingleOrDefault(r => r.Id == resourceLinkId));
        }

        Task<PartialList<LineItem>> ILti13CoreDataService.GetLineItemsAsync(string deploymentId, string contextId, int pageIndex, int limit, string? resourceId, string? resourceLinkId, string? tag, CancellationToken cancellationToken)
        {
            var lineItems = LineItems.Where(li => li.DeploymentId == deploymentId && li.ContextId == contextId && (resourceId == null || li.ResourceId == resourceId) && (resourceLinkId == null || li.ResourceLinkId == resourceLinkId) && (tag == null || li.Tag == tag)).ToList();

            return Task.FromResult(new PartialList<LineItem>
            {
                Items = [.. lineItems.Skip(pageIndex * limit).Take(limit)],
                TotalItems = lineItems.Count
            });
        }

        // deeplinking && assignmentgradeservices
        /// <summary>
        /// Saves a line item.
        /// </summary>
        /// <param name="lineItem">The line item to save.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The ID of the saved line item.</returns>
        public Task<string> SaveLineItemAsync(LineItem lineItem, CancellationToken cancellationToken = default)
        {
            var existingLineItem = LineItems.SingleOrDefault(x => x.Id == lineItem.Id);
            if (existingLineItem != null)
            {
                LineItems[LineItems.IndexOf(existingLineItem)] = lineItem;
                return Task.FromResult(lineItem.Id);
            }
            else
            {
                lineItem.Id = Guid.NewGuid().ToString();
                LineItems.Add(lineItem);
                return Task.FromResult(lineItem.Id);
            }
        }

        async Task<Attempt?> ILti13CoreDataService.GetAttemptAsync(string resourceLinkId, string userId, CancellationToken cancellationToken)
        {
            return await Task.FromResult(Attempts.SingleOrDefault(a => a.ResourceLinkId == resourceLinkId && a.UserId == userId));
        }

        Task<PartialList<Grade>> ILti13AssignmentGradeDataService.GetGradesAsync(string lineItemId, int pageIndex, int limit, string? userId, CancellationToken cancellationToken)
        {
            var grades = Grades.Where(x => x.LineItemId == lineItemId && (userId == null || x.UserId == userId)).ToList();

            return Task.FromResult(new PartialList<Grade>
            {
                Items = [.. grades.Skip(pageIndex * limit).Take(limit)],
                TotalItems = grades.Count
            });
        }

        Task<Grade?> ILti13CoreDataService.GetGradeAsync(string lineItemId, string userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Grades.SingleOrDefault(g => g.LineItemId == lineItemId && g.UserId == userId));
        }

        Task ILti13AssignmentGradeDataService.SaveGradeAsync(Grade grade, CancellationToken cancellationToken)
        {
            var existingGrade = Grades.SingleOrDefault(x => x.LineItemId == grade.LineItemId && x.UserId == grade.UserId);
            if (existingGrade != null)
            {
                Grades[Grades.IndexOf(existingGrade)] = grade;
            }
            else
            {
                Grades.Add(grade);
            }

            return Task.CompletedTask;
        }

        Task<ServiceToken?> ILti13CoreDataService.GetServiceTokenAsync(string clientId, string serviceTokenId, CancellationToken cancellationToken)
        {
            return Task.FromResult(ServiceTokens.FirstOrDefault(x => x.ClientId == clientId && x.Id == serviceTokenId));
        }

        Task ILti13CoreDataService.SaveServiceTokenAsync(ServiceToken serviceToken, CancellationToken cancellationToken)
        {
            var existing = ServiceTokens.SingleOrDefault(x => x.ClientId == serviceToken.ClientId && x.Id == serviceToken.Id);
            if (existing != null)
            {
                ServiceTokens[ServiceTokens.IndexOf(existing)] = serviceToken;
            }
            else
            {
                ServiceTokens.Add(serviceToken);
            }

            return Task.CompletedTask;
        }

        Task<IEnumerable<SecurityKey>> ILti13CoreDataService.GetPublicKeysAsync(string clientId, CancellationToken cancellationToken)
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

        Task<SecurityKey> ILti13CoreDataService.GetPrivateKeyAsync(string clientId, CancellationToken cancellationToken)
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

        Task<PartialList<(Membership, User)>> ILti13NameRoleProvisioningDataService.GetMembershipsAsync(string deploymentId, string contextId, int pageIndex, int limit, string? role, string? resourceLinkId, DateTime? asOfDate, CancellationToken cancellationToken)
        {
            if (ResourceLinks.Any(x => x.ContextId == contextId && x.DeploymentId == deploymentId && (resourceLinkId == null || resourceLinkId == x.Id)))
            {
                var memberships = Memberships.Where(m => m.ContextId == contextId && (role == null || m.Roles.Contains(role))).ToList();
                var users = Users.Where(u => memberships.Select(m => m.UserId).Contains(u.Id)).ToList();

                return Task.FromResult(new PartialList<(Membership, User)>
                {
                    Items = memberships.Join(users, m => m.UserId, u => u.Id, (m, u) => (m, u)).Skip(pageIndex * limit).Take(limit).ToList(),
                    TotalItems = memberships.Count
                });
            }

            return Task.FromResult(PartialList<(Membership, User)>.Empty);
        }

        Task<string> ILti13DeepLinkingDataService.SaveContentItemAsync(string deploymentId, string? contextId, ContentItem contentItem, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid().ToString();

            if (contentItem is LtiResourceLinkContentItem ci && contextId != null)
            {
                ResourceLinks.Add(new ResourceLink
                {
                    ContextId = contextId,
                    DeploymentId = deploymentId,
                    Id = id,
                    AvailableEndDateTime = ci.Available?.EndDateTime?.UtcDateTime,
                    AvailableStartDateTime = ci.Available?.StartDateTime?.UtcDateTime,
                    SubmissionEndDateTime = ci.Submission?.EndDateTime?.UtcDateTime,
                    SubmissionStartDateTime = ci.Submission?.StartDateTime?.UtcDateTime,
                    ClonedIdHistory = [],
                    Custom = ci.Custom,
                    Text = ci.Text,
                    Title = ci.Title,
                    Url = ci.Url
                });
            }

            return Task.FromResult(id);
        }



        Task<LineItem?> ILti13AssignmentGradeDataService.GetLineItemAsync(string lineItemId, CancellationToken cancellationToken)
        {
            return Task.FromResult(LineItems.SingleOrDefault(x => x.Id == lineItemId));
        }

        Task ILti13AssignmentGradeDataService.DeleteLineItemAsync(string lineItemId, CancellationToken cancellationToken)
        {
            LineItems.RemoveAll(i => i.Id == lineItemId);

            return Task.CompletedTask;
        }

        Task<CustomPermissions> ILti13CoreDataService.GetCustomPermissions(string deploymentId, string? contextId, string userId, string? actualUserId, CancellationToken cancellationToken)
        {
            return Task.FromResult(new CustomPermissions { UserId = true, UserUsername = true });
        }

        /// <summary>
        /// Gets the user permissions for a specific user.
        /// </summary>
        /// <param name="deploymentId">The deployment ID.</param>
        /// <param name="contextId">The context ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The user's permissions.</returns>
        public Task<UserPermissions> GetUserPermissionsAsync(string deploymentId, string? contextId, string userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new UserPermissions { UserId = userId, FamilyName = true, Name = true, GivenName = true });
        }

        /// <summary>
        /// Gets the user permissions for multiple users.
        /// </summary>
        /// <param name="deploymentId">The deployment ID.</param>
        /// <param name="contextId">The context ID.</param>
        /// <param name="userIds">The user IDs.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of user permissions.</returns>
        public Task<IEnumerable<UserPermissions>> GetUserPermissionsAsync(string deploymentId, string? contextId, IEnumerable<string> userIds, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(userIds.Select(x => new UserPermissions { UserId = x, FamilyName = true, Name = true, GivenName = true }));
        }
    }
}