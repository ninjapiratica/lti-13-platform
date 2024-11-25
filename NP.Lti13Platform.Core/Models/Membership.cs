namespace NP.Lti13Platform.Core.Models
{
    public class Membership
    {
        public required string ContextId { get; set; }

        public required string UserId { get; set; }

        public required MembershipStatus Status { get; set; }

        public required IEnumerable<string> Roles { get; set; }

        public required IEnumerable<string> MentoredUserIds { get; set; }
    }

    public enum MembershipStatus
    {
        Active,
        Inactive
    }
}
