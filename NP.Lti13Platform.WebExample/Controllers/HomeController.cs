using Microsoft.AspNetCore.Mvc;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using NP.Lti13Platform.DeepLinking;
using NP.Lti13Platform.DeepLinking.Services;

namespace NP.Lti13Platform.WebExample.Controllers;

public class HomeController(ILogger<HomeController> logger, IUrlService service, ILti13DeepLinkingUrlService deepLinkUrlService) : Controller
{
    public async Task<IResult> Index(CancellationToken cancellationToken)
    {
        var deploymentId = new DeploymentId("deploymentId");
        var contextId = new ContextId("contextId");
        var userId = new UserId("userId");
        var documentTarget = Lti13PresentationTargetDocuments.Window;
        var height = 200;
        var width = 250;
        var locale = "en-US";

        logger.LogInformation("LOGGING INFORMATION");

        return Results.Ok(new
        {
            deepLinkUrl = (await deepLinkUrlService.GetDeepLinkInitiationUrlAsync(
                deploymentId,
                userId,
                false,
                deepLinkUrl: null,
                actualUserId: null,
                contextId,
                new DeepLinkSettingsOverride { Title = "TiTlE", Text = "TEXT", Data = "data" },
                cancellationToken: cancellationToken)).AsForm("form1"),
            resourceLinkUrls = DataService.ResourceLinks
                .Select(async resourceLink => (await service.GetResourceLinkInitiationUrlAsync(
                    resourceLink.Id,
                    userId,
                    false,
                    launchPresentation: new LaunchPresentationOverride
                    {
                        DocumentTarget = documentTarget,
                        Height = height,
                        Width = width,
                        Locale = locale
                    },
                    cancellationToken: cancellationToken)).AsForm("form1"))
                .Select(t => t.Result)
        });
    }
}
