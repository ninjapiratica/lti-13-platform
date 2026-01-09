using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NP.Lti13Platform.AssignmentGradeServices.Configs;
using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.Test.AssignmentGradeServices.Services;

public class AssignmentGradeConfigServiceTests
{
    [Fact]
    public async Task GetConfigAsync_ReturnsConfigWithDefaultUriReplaced()
    {
        // Arrange
        var config = new ServicesConfig { ServiceAddress = ServicesConfig.DefaultUri };
        var optionsMonitorMock = new Mock<IOptionsMonitor<ServicesConfig>>();
        optionsMonitorMock.Setup(o => o.CurrentValue).Returns(config);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.com");
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
        var service = new DefaultLti13AssignmentGradeConfigService(optionsMonitorMock.Object, httpContextAccessorMock.Object);

        // Act
        var result = await service.GetConfigAsync(new ClientId("client"));

        // Assert
        Assert.Equal(new Uri("https://example.com"), result.ServiceAddress);
    }

    [Fact]
    public async Task GetConfigAsync_ReturnsConfigAsIs_WhenNotDefaultUri()
    {
        // Arrange
        var config = new ServicesConfig { ServiceAddress = new Uri("https://custom.com") };
        var optionsMonitorMock = new Mock<IOptionsMonitor<ServicesConfig>>();
        optionsMonitorMock.Setup(o => o.CurrentValue).Returns(config);
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var service = new DefaultLti13AssignmentGradeConfigService(optionsMonitorMock.Object, httpContextAccessorMock.Object);

        // Act
        var result = await service.GetConfigAsync(new ClientId("client"));

        // Assert
        Assert.Equal(config, result);
    }
}