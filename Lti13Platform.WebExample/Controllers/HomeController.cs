using Microsoft.AspNetCore.Mvc;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.DeepLinking;

namespace NP.Lti13PlatformExample.Controllers
{
    public class HomeController(ILogger<HomeController> logger, Service service, ICoreDataService dataService) : Controller
    {
        public async Task<IResult> Index()
        {
            var tool = await dataService.GetToolAsync("asdfasdf");
            var deployment = await dataService.GetDeploymentAsync("asdf");
            var context = await dataService.GetContextAsync(new Guid().ToString());
            var userId = "asdf";
            var documentTarget = Lti13PresentationTargetDocuments.Window;
            var height = 200;
            var width = 250;
            var locale = "en-US";

            var resourceLink = await dataService.GetResourceLinkAsync(new Guid().ToString());

            return Results.Ok(new
            {
                deepLinkUrl = service.GetDeepLinkInitiationUrl(tool!, deployment!.Id, context!.Id, userId, new DeepLinkSettingsOverride(null, null, null, null, null, null, null, "TiTlE", "TEXT", "data")), //new LaunchPresentation { DocumentTarget = documentTarget, Height = height, Width = width, Locale = locale, ReturnUrl = "" }),
                contentItemUrl = resourceLink != null ? service.GetResourceLinkInitiationUrl(tool!, deployment!.Id, context!.Id, resourceLink, userId) : null // new LaunchPresentation { DocumentTarget = documentTarget, Height = height, Width = width, Locale = locale, ReturnUrl = "" }) : null,
            });
        }
    }
}
