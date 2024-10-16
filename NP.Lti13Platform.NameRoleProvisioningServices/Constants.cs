namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    internal static class RouteNames
    {
        public const string GET_MEMBERSHIPS = "GET_MEMBERSHIPS";
    }

    internal static class Lti13ContentTypes
    {
        internal const string MembershipContainer = "application/vnd.ims.lti-nrps.v2.membershipcontainer+json";
    }

    public static class Lti13ServiceScopes
    {
        public const string MembershipReadOnly = "https://purl.imsglobal.org/spec/lti-nrps/scope/contextmembership.readonly";
    }
}
