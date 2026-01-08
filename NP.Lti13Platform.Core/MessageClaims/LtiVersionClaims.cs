using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.MessageClaims;

/// <summary>
/// Defines the contract for accessing the LTI version claim in an LTI message.
/// </summary>
public interface ILtiVersionClaims
{
    /// <summary>
    /// Gets or sets the LTI version.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/version")]
    public string LtiVersion { get; set; }
}

public static partial class ClaimsExtensions
{
    /// <summary>
    /// Sets the LTI version claim to "1.3.0" on the specified object.
    /// </summary>
    /// <remarks>This method modifies the <paramref name="obj"/> instance by setting its <c>LtiVersion</c> property. The returned object is the same instance passed in, allowing for method chaining.</remarks>
    /// <typeparam name="T">The type of the object that implements <see cref="ILtiVersionClaims"/>.</typeparam>
    /// <param name="obj">The object on which to set the LTI version claim.</param>
    /// <returns>The same object instance with its LTI version claim set to "1.3.0".</returns>
    public static T WithLtiVersionClaims<T>(
        this T obj)
        where T : ILtiVersionClaims
    {
        obj.LtiVersion = "1.3.0";

        return obj;
    }
}
