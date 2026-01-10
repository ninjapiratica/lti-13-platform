namespace NP.Lti13Platform.AssignmentGradeServices.Constants;

/// <summary>
/// Provides constants for service scopes used in assignment grade services.
/// </summary>
public static class ServiceScopes
{
    /// <summary>
    /// Scope for managing line items.
    /// </summary>
    public static readonly string LineItem = "https://purl.imsglobal.org/spec/lti-ags/scope/lineitem";

    /// <summary>
    /// Scope for read-only access to line items.
    /// </summary>
    public static readonly string LineItemReadOnly = "https://purl.imsglobal.org/spec/lti-ags/scope/lineitem.readonly";

    /// <summary>
    /// Scope for read-only access to results.
    /// </summary>
    public static readonly string ResultReadOnly = "https://purl.imsglobal.org/spec/lti-ags/scope/result.readonly";

    /// <summary>
    /// Scope for managing scores.
    /// </summary>
    public static readonly string Score = "https://purl.imsglobal.org/spec/lti-ags/scope/score";
}
