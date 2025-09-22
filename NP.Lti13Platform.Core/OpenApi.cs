using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace NP.Lti13Platform.Core
{
    /// <summary>
    /// Adds LTI 1.3 authorization support to the OpenAPI configuration.
    /// </summary>
    /// <remarks>This method registers a document transformer and an operation transformer to integrate LTI
    /// 1.3 authorization into the OpenAPI specification. The document transformer adds the necessary security scheme at
    /// the document level, while the operation transformer applies security requirements to operations that require
    /// authorization.</remarks>
    public static class OpenApi
    {
        private static readonly string SecuritySchemeId = "LTI 1.3 Bearer";

        /// <summary>
        /// Represents the name of the group associated with the OpenAPI configuration.
        /// </summary>
        public static readonly string GroupName = $"{typeof(OpenApi).FullName}.GoupName";

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
                options.ShouldInclude = (description) => description.GroupName == GroupName;
                options.AddDocumentTransformer<DocumentTransformer>();
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Title = "LTI 1.3 Platform API";
                    return Task.CompletedTask;
                });
                options.AddOperationTransformer<OperationTransformer>();
            });

            return services;
        }

        /// <summary>
        /// Adds security schemes to the OpenAPI document for LTI 1.3 authorization.
        /// </summary>
        public class DocumentTransformer : IOpenApiDocumentTransformer
        {
            /// <summary>
            /// Adds a predefined security scheme to the OpenAPI document if it is not already present.
            /// </summary>
            /// <remarks>This method ensures that the OpenAPI document includes a security scheme
            /// with the HTTP bearer authentication type. If the security scheme already exists
            /// in the document, no changes are made.</remarks>
            /// <param name="document">The <see cref="OpenApiDocument"/> to which the security scheme will be added.</param>
            /// <param name="context">The context for the OpenAPI document transformation. This parameter provides additional metadata or state for the transformation process.</param>
            /// <param name="cancellationToken">A token to monitor for cancellation requests. This can be used to cancel the operation if needed.</param>
            /// <returns>A task that represents the asynchronous operation. The task completes when the transformation is finished.</returns>
            public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
            {
                document.Components ??= new OpenApiComponents();

                if (!document.Components.SecuritySchemes.ContainsKey(SecuritySchemeId))
                {
                    document.Components.SecuritySchemes.Add(SecuritySchemeId, new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        In = ParameterLocation.Header,
                    });
                }

                return Task.CompletedTask;
            }
        }

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
                if (context.Description.GroupName == GroupName &&
                    context.Description.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any())
                {
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = SecuritySchemeId, Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                    });
                }

                return Task.CompletedTask;
            }
        }
    }
}
