using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.MessageClaims;

/// <summary>
/// Defines the contract for accessing the target link URI claim in an LTI 1.3 message.
/// </summary>
/// <remarks>The target link URI identifies the resource within the tool that the platform requests to launch.
/// Implementations should ensure that this value is a valid absolute URI as required by the LTI
/// specification.</remarks>
public interface ITargetLinkUriClaims
{
    /// <summary>
    /// Gets or sets the target link URI.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/target_link_uri")]
    public string TargetLinkUri { get; set; }
}

public static partial class ClaimsExtensions
{
    /// <summary>
    /// Sets the TargetLinkUri claim on the specified object and returns the updated instance.
    /// </summary>
    /// <typeparam name="T">The type of the object that implements ILtiTargetLinkUriClaims.</typeparam>
    /// <param name="obj">The object whose TargetLinkUri claim will be set.</param>
    /// <param name="targetLinkUri">The URI to assign to the TargetLinkUri claim. Cannot be null.</param>
    /// <returns>The same object instance with its TargetLinkUri claim set to the specified URI.</returns>
    public static T WithTargetLinkUriClaims<T>(
        this T obj,
        Uri targetLinkUri)
        where T : ITargetLinkUriClaims
    {
        obj.TargetLinkUri = targetLinkUri.OriginalString;

        return obj;
    }
}
