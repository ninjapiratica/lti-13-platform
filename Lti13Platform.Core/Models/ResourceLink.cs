namespace NP.Lti13Platform.Models
{
    public class ResourceLink
    {
        public required Guid Id { get; set; }

        public required Guid ContextId { get; set; }

        public string? Url { get; set; }

        public string? Description { get; set; }

        public string? Title { get; set; }
    }
}
