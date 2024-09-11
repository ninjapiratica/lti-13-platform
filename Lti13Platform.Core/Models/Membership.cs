namespace NP.Lti13Platform.Models
{
    public class Membership
    {
        public required MembershipStatus Status { get; set; }

        public required string ContextId { get; set; }

        public required string UserId { get; set; }

        public required IEnumerable<string> Roles { get; set; }
    }
}
