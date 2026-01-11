using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace NP.Lti13Platform.Core.OpenApi;

/// <summary>
/// Adds LTI 1.3 authorization support to the OpenAPI configuration.
/// </summary>
/// <remarks>This method registers a document transformer and an operation transformer to integrate LTI
/// 1.3 authorization into the OpenAPI specification. The document transformer adds the necessary security scheme at
/// the document level, while the operation transformer applies security requirements to operations that require
/// authorization.</remarks>
public static class Lti13OpenApi
{
    internal static readonly string SecuritySchemeId = "LTI 1.3 Bearer";

    /// <summary>
    /// Represents the name of the group associated with the OpenAPI configuration.
    /// </summary>
    public static readonly string GroupName = $"{typeof(Lti13OpenApi).FullName}.GoupName";

    /// <summary>
    /// Adds support for LTI 1.3 authorization to the OpenAPI options.
    /// </summary>
    /// <remarks>This method registers the necessary transformers to support LTI 1.3 authorization in the OpenAPI documentation.</remarks>
    /// <param name="options">The <see cref="OpenApiOptions"/> instance to configure.</param>
    /// <returns>The configured <see cref="OpenApiOptions"/> instance, enabling LTI 1.3 authorization.</returns>
    public static OpenApiOptions AddLti13Authorization(this OpenApiOptions options)
    {
        options.AddDocumentTransformer<DocumentTransformer>();
        options.AddOperationTransformer<OperationTransformer>();
        return options;
    }

    /// <summary>
    /// Configures the service collection to include OpenAPI documentation for LTI 1.3 endpoints.
    /// </summary>
    /// <remarks>This method adds a new OpenApi Document for all LTI 1.3 endpoints.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="documentName">The name of the OpenAPI document to create.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddLti13OpenApi(this IServiceCollection services, string documentName)
    {
        services.AddOpenApi(documentName, options =>
        {
            options.CreateSchemaReferenceId = (type) => type.Type.IsEnum || type.Type.GetCustomAttributes<StringIdAttribute>().Any() ? null : OpenApiOptions.CreateDefaultSchemaReferenceId(type);
            options.ShouldInclude = (description) => description.GroupName == GroupName;
            options.AddDocumentTransformer<DocumentTransformer>();
            options.AddOperationTransformer<OperationTransformer>();
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = "LTI 1.3 Platform API";
                return Task.CompletedTask;
            });
        });

        return services;
    }
}
