using Microsoft.Extensions.Options;
using NP.Lti13Platform.AssignmentGradeServices.Configs;

namespace NP.Lti13Platform.AssignmentGradeServices.Services
{
    internal class ServiceHelper(IOptionsMonitor<ServicesConfig> config) : IServiceHelper
    {
        public Task<ServicesConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default) => Task.FromResult(config.CurrentValue);
    }
}