namespace NP.Lti13Platform.NameRoleProvisioningServices;

/// <summary>
/// Defines route names used in the Name and Role Provisioning Services.
/// </summary>
internal static class RouteNames
{
    /// <summary>
    /// Route name for retrieving memberships.
    /// </summary>
    public static readonly string GET_MEMBERSHIPS = "GET_MEMBERSHIPS";
}

/// <summary>
/// Defines content types used in the LTI 1.3 Name and Role Provisioning Services.
/// </summary>
internal static class Lti13ContentTypes
{
    /// <summary>
    /// Content type for the membership container in LTI-NRPS v2.
    /// </summary>
    internal static readonly string MembershipContainer = "application/vnd.ims.lti-nrps.v2.membershipcontainer+json";
}

/// <summary>
/// Defines service scopes used in the LTI 1.3 platform.
/// </summary>
public static class Lti13ServiceScopes
{
    /// <summary>
    /// Scope for read-only access to context membership data.
    /// </summary>
    public static readonly string MembershipReadOnly = "https://purl.imsglobal.org/spec/lti-nrps/scope/contextmembership.readonly";
}
