namespace NP.Lti13Platform.AssignmentGradeServices;

/// <summary>
/// Route names need to be globally unique. To avoid the possibility of overlapping with other endpoints (outside of this library), the route names are made globally unique.
/// </summary>
internal static class RouteNames
{
    internal static readonly string GET_LINE_ITEMS = "d6ba4be1-8885-4fb8-9421-b9631b7cac07";
    internal static readonly string GET_LINE_ITEM = "82749fdb-ecec-4351-a90e-55ea4e0c1e21";
    internal static readonly string GET_LINE_ITEM_RESULTS = "8224f1d6-5e3f-4ee8-b62e-acaf8b644a39";
}

internal static class ContentTypes
{
    internal static readonly string LineItemContainer = "application/vnd.ims.lis.v2.lineitemcontainer+json";
    internal static readonly string LineItem = "application/vnd.ims.lis.v2.lineitem+json";
    internal static readonly string ResultContainer = "application/vnd.ims.lis.v2.resultcontainer+json";
    internal static readonly string Score = "application/vnd.ims.lis.v1.score+json";
}

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
