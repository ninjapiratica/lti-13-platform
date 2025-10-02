using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NP.Lti13Platform.Core.Populators;
using NP.Lti13Platform.NameRoleProvisioningServices.Services;
using System.Text.Json.Serialization;

namespace NP.Lti13Platform.NameRoleProvisioningServices.Populators;

/// <summary>
/// Defines an interface for a message containing service endpoints for LTI Name and Role Provisioning Services.
/// </summary>
public interface IServiceEndpoints
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
/// Populates service endpoints for LTI Name and Role Provisioning Services.
/// </summary>
public class ServiceEndpointsPopulator(LinkGenerator linkGenerator, ILti13NameRoleProvisioningConfigService nameRoleProvisioningService) : Populator<IServiceEndpoints>
{
    /// <summary>
    /// Populates service endpoints based on the message scope.
    /// </summary>
    /// <param name="obj">The object to populate.</param>
    /// <param name="scope">The message scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task PopulateAsync(IServiceEndpoints obj, MessageScope scope, CancellationToken cancellationToken = default)
    {
        if (scope.Tool.ServiceScopes.Contains(Lti13ServiceScopes.MembershipReadOnly) && scope.Context?.Id != null)
        {
            var config = await nameRoleProvisioningService.GetConfigAsync(scope.Tool.ClientId, cancellationToken);

            obj.NamesRoleService = new IServiceEndpoints.ServiceEndpoints
            {
                ContextMembershipsUrl = linkGenerator.GetUriByName(RouteNames.GET_MEMBERSHIPS, new { deploymentId = scope.Deployment.Id, contextId = scope.Context.Id }, config.ServiceAddress.Scheme, new HostString(config.ServiceAddress.Authority)) ?? string.Empty,
                ServiceVersions = ["2.0"]
            };
        }
    }
}
