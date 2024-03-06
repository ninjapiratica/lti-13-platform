using NP.Lti13Platform.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddLti13PlatformWeb();
builder.Services.AddTransient<IDataService, DataService>();


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

app.UseLti13PlatformWeb();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

internal class DataService : IDataService
{
    public Task<Lti13Client?> GetClientAsync(string clientId)
    {
        return Task.FromResult<Lti13Client?>(new Lti13Client { Id = "asdf", OidcInitiationUrl = "", RedirectUris = ["https://www.test.com"], Jwks = "" });
    }

    public Task<Lti13Context?> GetContextAsync(string contextId)
    {
        return Task.FromResult<Lti13Context?>(new Lti13Context { Id = "asdf", DeploymentId = "asdf", Label = "", Title = "", Types = [] });
    }

    public Task<Lti13Deployment?> GetDeploymentAsync(string deploymentId)
    {
        return Task.FromResult<Lti13Deployment?>(new Lti13Deployment { Id = "asdf", ClientId = "asdf" });
    }

    public Task<IEnumerable<string>> GetMentoredUserIdsAsync(string userId, Lti13Client client, Lti13Context? context)
    {
        return Task.FromResult<IEnumerable<string>>([]);
    }

    public Task<Lti13ResourceLink?> GetResourceLinkAsync(string resourceLinkId)
    {
        return Task.FromResult<Lti13ResourceLink?>(new Lti13ResourceLink { Id = "asdf", ContextId = "asdf" });
    }

    public Task<IEnumerable<string>> GetRolesAsync(string userId, Lti13Client client, Lti13Context? context)
    {
        return Task.FromResult<IEnumerable<string>>([]);
    }

    public Task<Lti13OpenIdUser?> GetUserAsync(Lti13Client client, string userId)
    {
        return Task.FromResult<Lti13OpenIdUser?>(new Lti13OpenIdUser { });
    }
}