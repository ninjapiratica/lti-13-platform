using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.AssignmentGradeServices
{
    public interface IAssignmentGradeServicesDataService
    {
        Task<LineItem?> GetLineItemAsync(string lineItemId);
        Task DeleteLineItemAsync(string lineItemId);
        Task<string> SaveLineItemAsync(LineItem lineItem);

        Task<PartialList<Grade>> GetGradesAsync(string lineItemId, int pageIndex, int limit, string? userId = null);
        Task SaveGradeAsync(Grade result);
    }
}
