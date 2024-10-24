using NP.Lti13Platform.AssignmentGradeServices.Configs;

namespace NP.Lti13Platform.AssignmentGradeServices.Services
{
    public interface IAssignmentGradeService
    {
        Task<ServicesConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default);
    }
}