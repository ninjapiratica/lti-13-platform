using Microsoft.Extensions.Options;
using Moq;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using Xunit;

namespace NP.Lti13Platform.Test.Core.Services;

public class Lti13PlatformServiceTests
{
    [Fact]
    public async Task GetPlatformAsync_ReturnsPlatform_WhenGuidIsNotEmpty()
    {
        // Arrange
        var platform = new Platform { Guid = "test-guid" };
        var optionsMonitorMock = new Mock<IOptionsMonitor<Platform>>();
        optionsMonitorMock.Setup(o => o.CurrentValue).Returns(platform);
        var service = new DefaultLti13PlatformService(optionsMonitorMock.Object);

        // Act
        var result = await service.GetPlatformAsync(new ClientId("client"));

        // Assert
        Assert.Equal(platform, result);
    }

    [Fact]
    public async Task GetPlatformAsync_ReturnsNull_WhenGuidIsEmpty()
    {
        // Arrange
        var platform = new Platform { Guid = "" };
        var optionsMonitorMock = new Mock<IOptionsMonitor<Platform>>();
        optionsMonitorMock.Setup(o => o.CurrentValue).Returns(platform);
        var service = new DefaultLti13PlatformService(optionsMonitorMock.Object);

        // Act
        var result = await service.GetPlatformAsync(new ClientId("client"));

        // Assert
        Assert.Null(result);
    }
}