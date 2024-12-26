namespace NP.Lti13Platform.NameRoleProvisioningServices
{
    internal static class RouteNames
    {
        public static readonly string GET_MEMBERSHIPS = "GET_MEMBERSHIPS";
    }

    internal static class Lti13ContentTypes
    {
        internal static readonly string MembershipContainer = "application/vnd.ims.lti-nrps.v2.membershipcontainer+json";
    }

    public static class Lti13ServiceScopes
    {
        public static readonly string MembershipReadOnly = "https://purl.imsglobal.org/spec/lti-nrps/scope/contextmembership.readonly";
    }
}
