using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace NP.Lti13Platform.Core.OpenApi;

/// <summary>
/// Adds security schemes to the OpenAPI document for LTI 1.3 authorization.
/// </summary>
public class DocumentTransformer : IOpenApiDocumentTransformer
{
    /// <summary>
    /// Adds a predefined security scheme to the OpenAPI document if it is not already present.
    /// </summary>
    /// <remarks>This method ensures that the OpenAPI document includes a security scheme
    /// with the HTTP bearer authentication type.</remarks>
    /// <param name="document">The <see cref="OpenApiDocument"/> to which the security scheme will be added.</param>
    /// <param name="context">The context for the OpenAPI document transformation. This parameter provides additional metadata or state for the transformation process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. This can be used to cancel the operation if needed.</param>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();

        if (!document.Components.SecuritySchemes.ContainsKey(Lti13OpenApi.SecuritySchemeId))
        {
            document.Components.SecuritySchemes.Add(Lti13OpenApi.SecuritySchemeId, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
            });
        }

        return Task.CompletedTask;
    }
}
