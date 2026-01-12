using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.Core.MessageHandlers;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.NameRoleProvisioningServices.Configs;
using NP.Lti13Platform.NameRoleProvisioningServices.Constants;
using NP.Lti13Platform.NameRoleProvisioningServices.Services;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices.MessageClaims;

/// <summary>
/// Defines an interface for a message containing service endpoints for LTI Name and Role Provisioning Services.
/// </summary>
public interface INameRoleServiceEndpointClaims
{
    /// <summary>
    /// Gets or sets the Names and Role Service endpoints.
    /// </summary>
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti-nrps/claim/namesroleservice")]
    public ServiceEndpoints? NamesRoleService { get; set; }

    /// <summary>
    /// Represents the service endpoints for LTI Name and Role Provisioning Services.
    /// </summary>
    public class ServiceEndpoints
    {
        /// <summary>
        /// Gets or sets the URL to access context memberships.
        /// </summary>
        [JsonPropertyName("context_memberships_url")]
        public required string ContextMembershipsUrl { get; set; }

        /// <summary>
        /// Gets or sets the supported service versions.
        /// </summary>
        [JsonPropertyName("service_versions")]
        public required IEnumerable<string> ServiceVersions { get; set; }
    }
}

/// <summary>
/// Provides extension methods for adding Names and Roles service endpoint claims to objects that support name and role service claims.
/// </summary>
public static partial class ClaimsExtensions
{
    /// <summary>
    /// Adds Names and Roles service endpoint claims to the specified object if the tool requests the MembershipReadOnly service scope.
    /// </summary>
    /// <remarks>This method only adds the Names and Roles service endpoint claims if the tool's service scopes include MembershipReadOnly.
    /// The returned object is the same instance as the input object, allowing for fluent configuration.</remarks>
    /// <typeparam name="T">The type of the object to which the Names and Roles service endpoint claims are added. Must implement INameRoleServiceEndpointClaims.</typeparam>
    /// <param name="obj">The object to which the Names and Roles service endpoint claims will be added.</param>
    /// <param name="tool">The tool instance containing the requested service scopes.</param>
    /// <param name="deploymentId">The deployment identifier used to generate the service endpoint URL.</param>
    /// <param name="contextId">The context identifier used to generate the service endpoint URL.</param>
    /// <param name="config">The configuration settings used to determine the service address for the endpoint.</param>
    /// <param name="linkGenerator">The link generator used to construct the service endpoint URL.</param>
    /// <returns>The same object instance with Names and Roles service endpoint claims added if applicable.</returns>
    public static T WithNameRoleServiceEndpointClaims<T>(
        this T obj,
        Tool tool,
        DeploymentId deploymentId,
        ContextId contextId,
        ServicesConfig config,
        LinkGenerator linkGenerator)
        where T : INameRoleServiceEndpointClaims
    {
        if (tool.ServiceScopes.Contains(Lti13ServiceScopes.MembershipReadOnly))
        {
            obj.NamesRoleService = new INameRoleServiceEndpointClaims.ServiceEndpoints
            {
                ContextMembershipsUrl = linkGenerator.GetUriByName(
                    RouteNames.GET_MEMBERSHIPS,
                    new
                    {
                        DeploymentId = deploymentId,
                        ContextId = contextId
                    },
                    config.ServiceAddress.Scheme,
                    new HostString(config.ServiceAddress.Authority)) ?? string.Empty,
                ServiceVersions = ["2.0"]
            };
        }

        return obj;
    }
}

internal class NameRoleServiceMessageExtension(
    INameRoleProvisioningConfigService configService,
    LinkGenerator linkGenerator)
    : ILtiResourceLinkMessageExtension<INameRoleServiceEndpointClaims>
{
    public async Task ExtendMessageAsync(INameRoleServiceEndpointClaims message, Tool tool, ResourceLink resourceLink, User? user, CancellationToken cancellationToken = default)
    {
        var config = await configService.GetConfigAsync(tool.ClientId, cancellationToken);

        message.WithNameRoleServiceEndpointClaims(
            tool,
            resourceLink.DeploymentId,
            resourceLink.ContextId,
            config,
            linkGenerator);
    }
}