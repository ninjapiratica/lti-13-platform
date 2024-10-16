namespace NP.Lti13Platform.AssignmentGradeServices
{
    internal static class RouteNames
    {
        internal const string GET_LINE_ITEMS = "GET_LINE_ITEMS";
        internal const string GET_LINE_ITEM = "GET_LINE_ITEM";
        internal const string GET_LINE_ITEM_RESULTS = "GET_LINE_ITEM_RESULTS";
    }

    internal static class ContentTypes
    {
        internal const string LineItemContainer = "application/vnd.ims.lis.v2.lineitemcontainer+json";
        internal const string LineItem = "application/vnd.ims.lis.v2.lineitem+json";
        internal const string ResultContainer = "application/vnd.ims.lis.v2.resultcontainer+json";
        internal const string Score = "application/vnd.ims.lis.v1.score+json";
    }

    public static class ServiceScopes
    {
        public const string LineItem = "https://purl.imsglobal.org/spec/lti-ags/scope/lineitem";
        public const string LineItemReadOnly = "https://purl.imsglobal.org/spec/lti-ags/scope/lineitem.readonly";
        public const string ResultReadOnly = "https://purl.imsglobal.org/spec/lti-ags/scope/result.readonly";
        public const string Score = "https://purl.imsglobal.org/spec/lti-ags/scope/score";
    }
}
