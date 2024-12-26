namespace NP.Lti13Platform.Core.Models;

public class ResourceLink
{
    public required string Id { get; set; }

    public required string DeploymentId { get; set; }

    public required string ContextId { get; set; }

    public string? Url { get; set; }

    public string? Title { get; set; }

    public string? Text { get; set; }

    public DateTime? AvailableStartDateTime { get; set; }

    public DateTime? AvailableEndDateTime { get; set; }

    public DateTime? SubmissionStartDateTime { get; set; }

    public DateTime? SubmissionEndDateTime { get; set; }

    public IEnumerable<string>? ClonedIdHistory { get; set; }

    public IDictionary<string, string>? Custom { get; set; }
}
