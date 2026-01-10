using NP.Lti13Platform.Core.Models;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.Core.MessageClaims;

/// <summary>
/// Defines a contract for accessing the deployment identifier claim used in LTI 1.3 integrations.
/// </summary>
/// <remarks>Implementations of this interface provide access to the deployment Id associated with an LTI 1.3 launch,
/// as specified by the IMS Global standard. The deployment Id uniquely identifies the context in which the tool is
/// deployed within the platform.</remarks>
public interface IDeploymentIdClaims
{
    /// <summary>
    /// Gets or sets the deployment Id.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/deployment_id")]
    public DeploymentId DeploymentId { get; set; }
}

public static partial class ClaimsExtensions
{
    /// <summary>
    /// Assigns the specified deployment ID to the <see cref="IDeploymentIdClaims"/> implementation and returns the updated object.
    /// </summary>
    /// <typeparam name="T">The type of object that implements <see cref="IDeploymentIdClaims"/>.</typeparam>
    /// <param name="obj">The object to which the deployment ID claim will be assigned. Must implement <see cref="IDeploymentIdClaims"/>.</param>
    /// <param name="deploymentId">The deployment ID to assign to the object's <c>DeploymentId</c> property.</param>
    /// <returns>The same object instance with its <c>DeploymentId</c> property set to the specified value.</returns>
    public static T WithDeploymentIdClaims<T>(
        this T obj,
        DeploymentId deploymentId)
        where T : IDeploymentIdClaims
    {
        obj.DeploymentId = deploymentId;

        return obj;
    }
}
