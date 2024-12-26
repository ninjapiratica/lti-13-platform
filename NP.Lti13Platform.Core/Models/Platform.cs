namespace NP.Lti13Platform.Core.Models;

public class Platform
{
    public required string Guid { get; set; }

    public string? ContactEmail { get; set; }

    public string? Description { get; set; }

    public string? Name { get; set; }

    public string? Url { get; set; }

    public string? ProductFamilyCode { get; set; }

    public string? Version { get; set; }
}
