namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents a membership in a context.
/// </summary>
public class Membership
{
    /// <summary>
    /// Gets or sets the context ID.
    /// </summary>
    public required string ContextId { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// Gets or sets the status of the membership.
    /// </summary>
    public required MembershipStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the roles of the member.
    /// </summary>
    public required IEnumerable<string> Roles { get; set; }

    /// <summary>
    /// Gets or sets the IDs of the users mentored by this member.
    /// </summary>
    public required IEnumerable<string> MentoredUserIds { get; set; }
}

/// <summary>
/// Represents the status of a membership.
/// </summary>
public enum MembershipStatus
{
    /// <summary>
    /// The membership is active.
    /// </summary>
    Active,
    /// <summary>
    /// The membership is inactive.
    /// </summary>
    Inactive
}
