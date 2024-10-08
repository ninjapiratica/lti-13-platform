using Microsoft.AspNetCore.Mvc;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.DeepLinking;

namespace NP.Lti13Platform.WebExample.Controllers
{
    public class HomeController(ILogger<HomeController> logger, Service service, ICoreDataService dataService) : Controller
    {
        public async Task<IResult> Index()
        {
            var tool = await dataService.GetToolAsync("clientId");
            var deployment = await dataService.GetDeploymentAsync("deploymentId");
            var context = await dataService.GetContextAsync("contextId");
            var userId = "userId";
            var documentTarget = Lti13PresentationTargetDocuments.Window;
            var height = 200;
            var width = 250;
            var locale = "en-US";

            logger.LogInformation("LOGGING INFORMATION");

            return Results.Ok(new
            {
                deepLinkUrl = service.GetDeepLinkInitiationUrl(tool!, deployment!.Id, userId, false, null, context!.Id, new DeepLinkSettingsOverride(null, null, null, null, null, null, null, "TiTlE", "TEXT", "data")), //new LaunchPresentation { DocumentTarget = documentTarget, Height = height, Width = width, Locale = locale, ReturnUrl = "" }),
                resourceLinkUrls = DataService.ResourceLinks
                    .Select(resourceLink => service.GetResourceLinkInitiationUrl(tool!, deployment!.Id, context!.Id, resourceLink, userId, false, launchPresentation: new LaunchPresentationOverride { DocumentTarget = documentTarget, Height = height, Width = width, Locale = locale, ReturnUrl = "" }))
            });
        }
    }
}
