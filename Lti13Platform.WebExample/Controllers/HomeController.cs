using Microsoft.AspNetCore.Mvc;
using NP.Lti13Platform;

namespace NP.Lti13PlatformExample.Controllers
{
    public class HomeController(ILogger<HomeController> logger, Service service, IDataService dataService) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;

        public async Task<IResult> Index()
        {
            var client = await dataService.GetClientAsync("asdf");
            var deployment = await dataService.GetDeploymentAsync("asdf");
            var context = await dataService.GetContextAsync("asdf");
            var userId = "asdf";
            var title = "TITLE";
            var text = "TEXT";
            var data = "DATA";
            var documentTarget = Lti13PresentationTargetDocuments.Window;
            var height = 200;
            var width = 250;
            var locale = "en-US";
            var returnUrl = "https://www.google.com";

            return Results.Ok(service.GetDeepLinkInitiationUrl(client!, deployment!, context!, userId, title, text, data, documentTarget, height, width, locale, returnUrl));
        }
    }
}
