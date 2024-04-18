using Microsoft.AspNetCore.Mvc;
using NP.Lti13Platform;
using NP.Lti13Platform.Models;

namespace NP.Lti13PlatformExample.Controllers
{
    public class HomeController(ILogger<HomeController> logger, Service service, IDataService dataService) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;

        public async Task<IResult> Index()
        {
            var client = await dataService.GetClientAsync(new Guid());
            var deployment = await dataService.GetDeploymentAsync(new Guid());
            var context = await dataService.GetContextAsync(new Guid());
            var userId = "asdf";
            var title = "TITLE";
            var text = "TEXT";
            var data = "DATA";
            var documentTarget = Lti13PresentationTargetDocuments.Window;
            var height = 200;
            var width = 250;
            var locale = "en-US";

            var resourceLink = await dataService.GetContentItemAsync<LtiResourceLinkContentItem>(new Guid());

            return Results.Ok(new
            {
                deepLinkUrl = service.GetDeepLinkInitiationUrl(client!, deployment!, context!, userId, title, text, data, documentTarget, height, width, locale),
                contentItemUrl = resourceLink != null ? service.GetResourceLinkInitiationUrl(client!, deployment!, resourceLink!, userId, documentTarget, height, width, locale) : null
            });
        }
    }
}
