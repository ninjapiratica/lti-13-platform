using Microsoft.Extensions.Logging;
using Moq;
using NP.Lti13Platform.Core.MessageClaims;
using NP.Lti13Platform.Core.MessageHandlers;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using Xunit;

namespace NP.Lti13Platform.Test.Core.MessageHandlers;

public class Lti13ResourceLinkRequestMessageHandlerTests
{
    [Fact]
    public async Task HandleLtiMessageAsync_ReturnsNone_WhenLtiMessageHintIsNull()
    {
        // Arrange
        var coreDataServiceMock = new Mock<ILti13CoreDataService>();
        var dataServiceMock = new Mock<ILti13ResourceLinkMessageDataService>();
        var tokenConfigServiceMock = new Mock<ILti13TokenConfigService>();
        var platformServiceMock = new Mock<ILti13PlatformService>();
        var extensions = Enumerable.Empty<ILti13ResourceLinkMessageExtension>();
        var loggerMock = new Mock<ILogger<Lti13ResourceLinkRequestMessageHandler>>();

        var handler = new Lti13ResourceLinkRequestMessageHandler(
            coreDataServiceMock.Object,
            dataServiceMock.Object,
            tokenConfigServiceMock.Object,
            platformServiceMock.Object,
            extensions,
            loggerMock.Object);

        var tool = new Tool { ClientId = new ClientId("client"), OidcInitiationUrl = new Uri("https://example.com"), LaunchUrl = new Uri("https://example.com") };

        // Act
        var result = await handler.HandleLtiMessageAsync("loginHint", null, tool, "nonce");

        // Assert
        Assert.IsType<LtiMessageResult.NoneResult>(result);
    }

    [Fact]
    public async Task HandleLtiMessageAsync_ReturnsNone_WhenLtiMessageHintIsInvalid()
    {
        // Arrange
        var coreDataServiceMock = new Mock<ILti13CoreDataService>();
        var dataServiceMock = new Mock<ILti13ResourceLinkMessageDataService>();
        var tokenConfigServiceMock = new Mock<ILti13TokenConfigService>();
        var platformServiceMock = new Mock<ILti13PlatformService>();
        var extensions = Enumerable.Empty<ILti13ResourceLinkMessageExtension>();
        var loggerMock = new Mock<ILogger<Lti13ResourceLinkRequestMessageHandler>>();

        var handler = new Lti13ResourceLinkRequestMessageHandler(
            coreDataServiceMock.Object,
            dataServiceMock.Object,
            tokenConfigServiceMock.Object,
            platformServiceMock.Object,
            extensions,
            loggerMock.Object);

        var tool = new Tool { ClientId = new ClientId("client"), OidcInitiationUrl = new Uri("https://example.com"), LaunchUrl = new Uri("https://example.com") };

        // Act
        var result = await handler.HandleLtiMessageAsync("loginHint", "invalid", tool, "nonce");

        // Assert
        Assert.IsType<LtiMessageResult.NoneResult>(result);
    }

    [Fact]
    public async Task HandleLtiMessageAsync_ReturnsNone_WhenLoginHintIsInvalid()
    {
        // Arrange
        var coreDataServiceMock = new Mock<ILti13CoreDataService>();
        var dataServiceMock = new Mock<ILti13ResourceLinkMessageDataService>();
        var tokenConfigServiceMock = new Mock<ILti13TokenConfigService>();
        var platformServiceMock = new Mock<ILti13PlatformService>();
        var extensions = Enumerable.Empty<ILti13ResourceLinkMessageExtension>();
        var loggerMock = new Mock<ILogger<Lti13ResourceLinkRequestMessageHandler>>();

        var handler = new Lti13ResourceLinkRequestMessageHandler(
            coreDataServiceMock.Object,
            dataServiceMock.Object,
            tokenConfigServiceMock.Object,
            platformServiceMock.Object,
            extensions,
            loggerMock.Object);

        var tool = new Tool { ClientId = new ClientId("client"), OidcInitiationUrl = new Uri("https://example.com"), LaunchUrl = new Uri("https://example.com") };

        // Act
        var result = await handler.HandleLtiMessageAsync("invalid", "LtiResourceLinkRequest|resourceId|", tool, "nonce");

        // Assert
        Assert.IsType<LtiMessageResult.NoneResult>(result);
    }
}