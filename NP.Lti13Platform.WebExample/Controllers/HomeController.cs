using Microsoft.AspNetCore.Mvc;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.MessageHandlers;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.DeepLinking;
using NP.Lti13Platform.DeepLinking.MessageHandlers;

namespace NP.Lti13Platform.WebExample.Controllers;

public class HomeController(ILogger<HomeController> logger, ILti13ResourceLinkRequestMessageHandler service, ILti13DeepLinkingRequestMessageHandler deepLinkUrlService) : Controller
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
            deepLinkUrl = (await deepLinkUrlService.GetLtiLaunchAsync(
                deploymentId,
                contextId,
                userId: userId,
                actualUserId: null,
                isAnonymous: false,
                deepLinkingSettingsOverride: new DeepLinkingSettingsOverride { Title = "TiTlE", Text = "TEXT", Data = "data" },
                cancellationToken: cancellationToken))!.AsUri(),
            deepLinkForm = (await deepLinkUrlService.GetLtiLaunchAsync(
                deploymentId,
                contextId,
                userId: userId,
                actualUserId: null,
                isAnonymous: false,
                deepLinkingSettingsOverride: new DeepLinkingSettingsOverride { Title = "TiTlE", Text = "TEXT", Data = "data" },
                cancellationToken: cancellationToken))!.AsForm("form1"),
            resourceLinkUrls = DataService.ResourceLinks
                .Select(async resourceLink => (await service.GetLtiLaunchAsync(
                    resourceLink.Id,
                    userId,
                    isAnonymous: false,
                    launchPresentationOverride: new LaunchPresentationOverride
                    {
                        DocumentTarget = documentTarget,
                        Height = height,
                        Width = width,
                        Locale = locale
                    },
                    cancellationToken: cancellationToken))!.AsForm("form1"))
                .Select(t => t.Result)
        });
    }
}
