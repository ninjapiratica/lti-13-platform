namespace NP.Lti13Platform.AssignmentGradeServices;

internal static class RouteNames
{
    internal static readonly string GET_LINE_ITEMS = "GET_LINE_ITEMS";
    internal static readonly string GET_LINE_ITEM = "GET_LINE_ITEM";
    internal static readonly string GET_LINE_ITEM_RESULTS = "GET_LINE_ITEM_RESULTS";
}

internal static class ContentTypes
{
    internal static readonly string LineItemContainer = "application/vnd.ims.lis.v2.lineitemcontainer+json";
    internal static readonly string LineItem = "application/vnd.ims.lis.v2.lineitem+json";
    internal static readonly string ResultContainer = "application/vnd.ims.lis.v2.resultcontainer+json";
    internal static readonly string Score = "application/vnd.ims.lis.v1.score+json";
}

public static class ServiceScopes
{
    public static readonly string LineItem = "https://purl.imsglobal.org/spec/lti-ags/scope/lineitem";
    public static readonly string LineItemReadOnly = "https://purl.imsglobal.org/spec/lti-ags/scope/lineitem.readonly";
    public static readonly string ResultReadOnly = "https://purl.imsglobal.org/spec/lti-ags/scope/result.readonly";
    public static readonly string Score = "https://purl.imsglobal.org/spec/lti-ags/scope/score";
}
