using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace NP.Lti13Platform.Core.OpenApi;

/// <summary>
/// Adds security requirements to OpenAPI operations that require authorization.
/// </summary>
public class OperationTransformer : IOpenApiOperationTransformer
{
    /// <summary>
    /// Modifies the provided <see cref="OpenApiOperation"/> to include security requirements based on the endpoint's metadata.
    /// </summary>
    /// <remarks>If the endpoint metadata includes an <see cref="AuthorizeAttribute"/>, the method adds a security requirement to the operation.</remarks>
    /// <param name="operation">The OpenAPI operation to be transformed.</param>
    /// <param name="context">The context containing metadata about the current operation, including the action descriptor.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes when the transformation is finished.</returns>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.GroupName == Lti13OpenApi.GroupName &&
            context.Description.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any())
        {
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = Lti13OpenApi.SecuritySchemeId, Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
            });
        }

        return Task.CompletedTask;
    }
}