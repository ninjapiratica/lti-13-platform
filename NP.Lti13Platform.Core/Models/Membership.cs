namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents a membership in a context.
/// </summary>
public class Membership
{
    /// <summary>
    /// The unique identifier of the context in which the membership exists, as defined by the LTI 1.3 specification.
    /// </summary>
    public required ContextId ContextId { get; set; }

    /// <summary>
    /// The unique identifier of the user who is a member of the context, as defined by the LTI 1.3 specification.
    /// </summary>
    public required UserId UserId { get; set; }

    /// <summary>
    /// The status of the membership (active or inactive) as defined by the LTI 1.3 specification.
    /// </summary>
    public required MembershipStatus Status { get; set; }

    /// <summary>
    /// The roles assigned to the member in the context, as defined by the LTI 1.3 specification.
    /// </summary>
    public required IEnumerable<string> Roles { get; set; }

    /// <summary>
    /// The IDs of the users mentored by this user, as defined by the LTI 1.3 specification.
    /// </summary>
    public required IEnumerable<UserId> MentoredUserIds { get; set; }
}

/// <summary>
/// Represents the status of a membership.
/// </summary>
public enum MembershipStatus
{
    /// <summary>
    /// The membership is active as defined by the LTI 1.3 specification.
    /// </summary>
    Active,
    /// <summary>
    /// The membership is inactive as defined by the LTI 1.3 specification.
    /// </summary>
    Inactive
}
