using Microsoft.Extensions.Options;
using Moq;
using NP.Lti13Platform.Core.Configs;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using Xunit;

namespace NP.Lti13Platform.Test.Core.Services;

public class Lti13TokenConfigServiceTests
{
    [Fact]
    public async Task GetTokenConfigAsync_ReturnsCurrentValue()
    {
        // Arrange
        var config = new TokenConfig { Issuer = new Uri("https://example.com") };
        var optionsMonitorMock = new Mock<IOptionsMonitor<TokenConfig>>();
        optionsMonitorMock.Setup(o => o.CurrentValue).Returns(config);
        var service = new DefaultLti13TokenConfigService(optionsMonitorMock.Object);

        // Act
        var result = await service.GetTokenConfigAsync(new ClientId("client"));

        // Assert
        Assert.Equal(config, result);
    }
}