using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.AssignmentGradeServices.Services
{
    public interface ILti13AssignmentGradeDataService
    {
        Task<LineItem?> GetLineItemAsync(string lineItemId, CancellationToken cancellationToken = default);
        Task DeleteLineItemAsync(string lineItemId, CancellationToken cancellationToken = default);
        Task<string> SaveLineItemAsync(LineItem lineItem, CancellationToken cancellationToken = default);

        Task<PartialList<Grade>> GetGradesAsync(string lineItemId, int pageIndex, int limit, string? userId = null, CancellationToken cancellationToken = default);
        Task SaveGradeAsync(Grade result, CancellationToken cancellationToken = default);
    }
}
