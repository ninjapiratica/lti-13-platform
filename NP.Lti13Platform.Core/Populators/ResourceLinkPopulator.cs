using NP.Lti13Platform.Core.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.Populators;

/// <summary>
/// Defines the contract for a resource link message in LTI 1.3.
/// </summary>
public interface IResourceLinkMessage : ILaunchPresentationMessage
{
    /// <summary>
    /// Gets or sets the LTI version.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/version")]
    public string LtiVersion { get; set; }

    /// <summary>
    /// Gets or sets the deployment Id.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/deployment_id")]
    public DeploymentId DeploymentId { get; set; }

    /// <summary>
    /// Gets or sets the target link URI.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/target_link_uri")]
    public string TargetLinkUri { get; set; }

    /// <summary>
    /// Gets or sets the resource link information.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/resource_link")]
    public ResourceLinkMessage ResourceLink { get; set; }

    /// <summary>
    /// Represents resource link information in an LTI message.
    /// </summary>
    public class ResourceLinkMessage
    {
        /// <summary>
        /// Gets or sets the resource link Id.
        /// </summary>
        [JsonPropertyName("id")]
        public required ContentItemId Id { get; set; }

        /// <summary>
        /// Gets or sets the resource link description.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the resource link title.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }
}

/// <summary>
/// Populates a resource link message with information from the message scope.
/// </summary>
public class ResourceLinkPopulator() : Populator<IResourceLinkMessage>
{
    /// <summary>
    /// Populates a resource link message with information from the message scope.
    /// </summary>
    /// <param name="obj">The resource link message to populate.</param>
    /// <param name="scope">The message scope containing the resource link information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the resource link in the scope is null.</exception>
    public override async Task PopulateAsync(IResourceLinkMessage obj, MessageScope scope, CancellationToken cancellationToken = default)
    {
        if (scope.ResourceLink == null)
        {
            throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.ResourceLink)}");
        }

        obj.LtiVersion = "1.3.0";
        obj.DeploymentId = scope.Deployment.Id;

        obj.TargetLinkUri = (scope.ResourceLink.Url ?? scope.Tool.LaunchUrl).OriginalString;
        obj.ResourceLink = new IResourceLinkMessage.ResourceLinkMessage
        {
            Id = scope.ResourceLink.Id,
            Description = scope.ResourceLink.Text,
            Title = scope.ResourceLink.Title
        };

        if (!string.IsNullOrWhiteSpace(scope.MessageHint))
        {
            var launchPresentation = JsonSerializer.Deserialize<LaunchPresentationOverride>(Encoding.UTF8.GetString(Convert.FromBase64String(scope.MessageHint)));

            if (launchPresentation != null)
            {
                obj.LaunchPresentation = new ILaunchPresentationMessage.LaunchPresentationDefinition
                {
                    DocumentTarget = launchPresentation.DocumentTarget,
                    Height = launchPresentation.Height,
                    Locale = launchPresentation.Locale,
                    ReturnUrl = launchPresentation.ReturnUrl,
                    Width = launchPresentation.Width,
                };
            }
        }

        await Task.CompletedTask;
    }
}
