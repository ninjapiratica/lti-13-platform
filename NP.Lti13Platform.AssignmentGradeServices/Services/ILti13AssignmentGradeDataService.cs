using NP.Lti13Platform.Core.Models;

namespace NP.Lti13Platform.AssignmentGradeServices.Services;

/// <summary>
/// Defines the contract for a service that manages assignment and grade data for LTI 1.3 integrations.
/// </summary>
public interface ILti13AssignmentGradeDataService
{
    /// <summary>
    /// Retrieves a line item by its identifier.
    /// </summary>
    /// <param name="lineItemId">The identifier of the line item to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the line item if found; otherwise, null.</returns>
    Task<LineItem?> GetLineItemAsync(LineItemId lineItemId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a line item by its identifier.
    /// </summary>
    /// <param name="lineItemId">The identifier of the line item to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteLineItemAsync(LineItemId lineItemId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves a line item to the data store.
    /// </summary>
    /// <param name="lineItem">The line item to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identifier of the saved line item.</returns>
    Task<LineItemId> SaveLineItemAsync(LineItem lineItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves grades for a specified line item.
    /// </summary>
    /// <param name="lineItemId">The identifier of the line item to retrieve grades for.</param>
    /// <param name="pageIndex">The index of the page to retrieve.</param>
    /// <param name="limit">The maximum number of grades to retrieve.</param>
    /// <param name="userId">Optional. The identifier of a specific user to retrieve grades for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a partial list of grades.</returns>
    Task<PartialList<Grade>> GetGradesAsync(LineItemId lineItemId, int pageIndex, int limit, UserId? userId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves a grade to the data store.
    /// </summary>
    /// <param name="result">The grade to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveGradeAsync(Grade result, CancellationToken cancellationToken = default);
}
