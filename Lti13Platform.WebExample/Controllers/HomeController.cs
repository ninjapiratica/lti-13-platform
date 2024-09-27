using Microsoft.AspNetCore.Mvc;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.DeepLinking;

namespace NP.Lti13PlatformExample.Controllers
{
    public class HomeController(ILogger<HomeController> logger, Service service, IDataService dataService) : Controller
    {
        public async Task<IResult> Index()
        {
            var tool = await dataService.GetToolAsync("asdf");
            var deployment = await dataService.GetDeploymentAsync("asdf");
            var context = await dataService.GetContextAsync(new Guid().ToString());
            var userId = "asdf";
            var documentTarget = Lti13PresentationTargetDocuments.Window;
            var height = 200;
            var width = 250;
            var locale = "en-US";

            var resourceLink = await dataService.GetContentItemAsync<LtiResourceLinkContentItem>(new Guid().ToString());

            return Results.Ok(new
            {
                deepLinkUrl = service.GetDeepLinkInitiationUrl(tool!, deployment!.Id, context!.Id, userId, null), //new LaunchPresentation { DocumentTarget = documentTarget, Height = height, Width = width, Locale = locale, ReturnUrl = "" }),
                contentItemUrl = resourceLink != null ? service.GetResourceLinkInitiationUrl(tool!, resourceLink, userId) : null // new LaunchPresentation { DocumentTarget = documentTarget, Height = height, Width = width, Locale = locale, ReturnUrl = "" }) : null,
            });
        }
    }
}
