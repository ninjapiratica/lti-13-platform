using Microsoft.Extensions.Options;

namespace NP.Lti13Platform.AssignmentGradeServices
{
    public interface IAssignmentGradeService
    {
        Task<Lti13AssignmentGradeServicesConfig> GetConfigAsync(string clientId);
    }

    internal class AssignmentGradeService(IOptionsMonitor<Lti13AssignmentGradeServicesConfig> config) : IAssignmentGradeService
    {
        public virtual Task<Lti13AssignmentGradeServicesConfig> GetConfigAsync(string clientId) => Task.FromResult(config.CurrentValue);
    }
}