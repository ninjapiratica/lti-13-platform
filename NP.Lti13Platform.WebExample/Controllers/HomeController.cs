using Microsoft.AspNetCore.Mvc;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using NP.Lti13Platform.DeepLinking;

namespace NP.Lti13Platform.WebExample.Controllers;

public class HomeController(ILogger<HomeController> logger, IUrlServiceHelper service, ILti13CoreDataService dataService) : Controller
{
    public async Task<IResult> Index(CancellationToken cancellationToken)
    {
        var tool = await dataService.GetToolAsync(new ClientId("clientId"), cancellationToken);
        var deployment = await dataService.GetDeploymentAsync("deploymentId", cancellationToken);
        var context = await dataService.GetContextAsync("contextId", cancellationToken);
        var userId = "userId";
        var documentTarget = Lti13PresentationTargetDocuments.Window;
        var height = 200;
        var width = 250;
        var locale = "en-US";

        logger.LogInformation("LOGGING INFORMATION");

        return Results.Ok(new
        {
            deepLinkUrl = await service.GetDeepLinkInitiationUrlAsync(
                tool!,
                deployment!.Id,
                userId,
                false,
                null,
                context!.Id,
                new DeepLinkSettingsOverride { Title = "TiTlE", Text = "TEXT", Data = "data" },
                cancellationToken: cancellationToken),
            resourceLinkUrls = DataService.ResourceLinks
                .Select(async resourceLink => await service.GetResourceLinkInitiationUrlAsync(
                    tool!,
                    deployment!.Id,
                    context!.Id,
                    resourceLink,
                    userId,
                    false,
                    launchPresentation: new LaunchPresentationOverride
                    {
                        DocumentTarget = documentTarget,
                        Height = height,
                        Width = width,
                        Locale = locale
                    },
                    cancellationToken: cancellationToken))
                .Select(t => t.Result)
        });
    }
}
