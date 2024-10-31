using NP.Lti13Platform.AssignmentGradeServices.Configs;

namespace NP.Lti13Platform.AssignmentGradeServices.Services
{
    public interface ILti13AssignmentGradeConfigService
    {
        Task<ServicesConfig> GetConfigAsync(string clientId, CancellationToken cancellationToken = default);
    }
}