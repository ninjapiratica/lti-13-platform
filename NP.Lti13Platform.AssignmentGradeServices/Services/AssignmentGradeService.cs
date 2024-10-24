using Microsoft.Extensions.Options;
using NP.Lti13Platform.AssignmentGradeServices.Configs;

namespace NP.Lti13Platform.AssignmentGradeServices.Services
{
    internal class AssignmentGradeService(IOptionsMonitor<ServicesConfig> config) : IAssignmentGradeService
    {
        public Task<ServicesConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default) => Task.FromResult(config.CurrentValue);
    }
}