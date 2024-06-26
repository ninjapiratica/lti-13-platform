﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NP.Lti13Platform.Extensions;
using NP.Lti13Platform.Models;
using System.Collections;
using System.Collections.ObjectModel;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Web;

namespace NP.Lti13Platform
{
    internal static class RouteNames
    {
        public const string GET_LINE_ITEMS = "GET_LINE_ITEMS";
        public const string GET_LINE_ITEM = "GET_LINE_ITEM";
        public const string GET_LINE_ITEM_RESULTS = "GET_LINE_ITEM_RESULTS";
        public const string DEEP_LINKING_RESPONSE = "DEEP_LINKING_RESPONSE";
    }

    internal class LtiMessageTypeResolver : DefaultJsonTypeInfoResolver
    {
        private static readonly HashSet<Type> derivedTypes = [];

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var jsonTypeInfo = base.GetTypeInfo(type, options);

            var baseType = typeof(LtiMessage);
            if (jsonTypeInfo.Type == baseType)
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    IgnoreUnrecognizedTypeDiscriminators = true,
                };

                foreach (var derivedType in derivedTypes)
                {
                    jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType));
                }
            }

            return jsonTypeInfo;
        }

        public static void AddDerivedType<T>() where T : LtiMessage
        {
            derivedTypes.Add(typeof(T));
        }
    }

    public static class Startup
    {
        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    (typeInfo) =>
                    {
                        foreach(var prop in typeInfo.Properties.Where(p => p.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(p.PropertyType)))
                        {
                            prop.ShouldSerialize = (obj, val) => val is IEnumerable e && e.GetEnumerator().MoveNext();
                        }
                    }
                }
            }
        };

        public static IServiceCollection AddLti13Platform(this IServiceCollection services, Action<Lti13PlatformConfig> configure)
        {
            services.Configure(configure);

            services.AddTransient<Service>();
            services.AddTransient<IPlatformService, PlatformService>();
            services.AddKeyedTransient(typeof(Startup), (sp, key) => sp.GetRequiredService<ILoggerFactory>().CreateLogger("NP.Lti13Platform"));

            services.AddMessageHandler<DeepLinkingMessageHandler, LtiDeepLinkingMessage>(Lti13MessageType.LtiDeepLinkingRequest);
            services.AddMessageHandler<ResourceLinkMessageHandler, LtiResourceLinkMessage>(Lti13MessageType.LtiResourceLinkRequest);

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, LtiServicesAuthHandler>(LtiServicesAuthHandler.SchemeName, null);

            services.AddHttpContextAccessor();

            return services;
        }

        public static IServiceCollection AddMessageHandler<T, U>(this IServiceCollection services, string messageType) where T : class, IMessageHandler where U : LtiMessage
        {
            services.AddKeyedTransient<IMessageHandler, T>(messageType);

            LtiMessageTypeResolver.AddDerivedType<U>();

            return services;
        }

        public static IEndpointRouteBuilder UseLti13Platform(this IEndpointRouteBuilder app, Action<Lti13PlatformEndpointsConfig>? configure = null)
        {
            var config = new Lti13PlatformEndpointsConfig();
            configure?.Invoke(config);

            app.MapGet(config.JwksUrl,
                async (IDataService dataService) =>
                {
                    var keys = await dataService.GetPublicKeysAsync();
                    var keySet = new JsonWebKeySet();

                    foreach (var key in keys)
                    {
                        var jwk = JsonWebKeyConverter.ConvertFromSecurityKey(key);
                        jwk.Use = JsonWebKeyUseNames.Sig;
                        jwk.Alg = SecurityAlgorithms.RsaSha256;
                        keySet.Keys.Add(jwk);
                    }

                    return Results.Json(keySet, jsonOptions);
                });

            app.MapGet(config.AuthorizationUrl,
                async ([AsParameters] AuthenticationRequest request, IServiceProvider serviceProvider, LinkGenerator linkGenerator, HttpContext httpContext, IOptionsMonitor<Lti13PlatformConfig> config, IDataService dataService) => await AuthenticationHandler.HandleAsync(serviceProvider, dataService, request));

            app.MapPost(config.AuthorizationUrl,
                async ([FromForm] AuthenticationRequest request, IServiceProvider serviceProvider, LinkGenerator linkGenerator, HttpContext httpContext, IOptionsMonitor<Lti13PlatformConfig> config, IDataService dataService) => await AuthenticationHandler.HandleAsync(serviceProvider, dataService, request))
                .DisableAntiforgery();

            app.MapPost(config.DeepLinkingResponseUrl,
                async ([FromForm] DeepLinkResponseRequest request, string? contextId, [FromKeyedServices(typeof(Startup))] ILogger logger, IOptionsMonitor<Lti13PlatformConfig> config, IDataService dataService, IDeepLinkContentHandler deepLinkContentHandler) =>
                {
                    const string DEEP_LINKING_SPEC = "https://www.imsglobal.org/spec/lti-dl/v2p0/#deep-linking-response-message";
                    const string INVALID_CLIENT = "invalid_client";
                    const string INVALID_REQUEST = "invalid_request";
                    const string JWT_REQUIRED = "JWT is required";
                    const string DEPLOYMENT_ID_REQUIRED = "deployment_id is required";
                    const string CLIENT_ID_REQUIRED = "client_id is required";
                    const string DEPLOYMENT_ID_INVALID = "deployment_id is invalid";
                    const string MESSAGE_TYPE_INVALID = "message_type is invalid";
                    const string VERSION_INVALID = "version is invalid";

                    if (string.IsNullOrWhiteSpace(request.Jwt))
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = JWT_REQUIRED, Error_Uri = DEEP_LINKING_SPEC });
                    }

                    var jwt = new JsonWebToken(request.Jwt);

                    if (!jwt.TryGetClaim("https://purl.imsglobal.org/spec/lti/claim/deployment_id", out var deploymentIdClaim))
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = DEPLOYMENT_ID_REQUIRED, Error_Uri = DEEP_LINKING_SPEC });
                    }

                    var deployment = await dataService.GetDeploymentAsync(deploymentIdClaim.Value);
                    if (deployment == null)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = DEPLOYMENT_ID_INVALID, Error_Uri = DEEP_LINKING_SPEC });
                    }

                    var tool = await dataService.GetToolAsync(deployment.ClientId);
                    if (tool?.Jwks == null)
                    {
                        return Results.NotFound(new { Error = INVALID_CLIENT, Error_Description = CLIENT_ID_REQUIRED, Error_Uri = DEEP_LINKING_SPEC });
                    }

                    var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(request.Jwt, new TokenValidationParameters
                    {
                        IssuerSigningKeys = await tool.Jwks.GetKeysAsync(),
                        ValidAudience = config.CurrentValue.Issuer,
                        ValidIssuer = tool.ClientId.ToString()
                    });

                    if (!validatedToken.IsValid)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = validatedToken.Exception.Message, Error_Uri = DEEP_LINKING_SPEC });
                    }

                    if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/message_type", out var messageType) || (string)messageType != "LtiDeepLinkingResponse")
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = MESSAGE_TYPE_INVALID, Error_Uri = DEEP_LINKING_SPEC });
                    }

                    if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/version", out var version) || (string)version != "1.3.0")
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = VERSION_INVALID, Error_Uri = DEEP_LINKING_SPEC });
                    }

                    var response = new DeepLinkResponse
                    {
                        Data = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/data")?.Value,
                        Message = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/msg")?.Value,
                        Log = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/log")?.Value,
                        ErrorMessage = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errormsg")?.Value,
                        ErrorLog = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errorlog")?.Value,
                        ContentItems = validatedToken.ClaimsIdentity.FindAll("https://purl.imsglobal.org/spec/lti-dl/claim/content_items")
                            .Select(x =>
                            {
                                var type = JsonDocument.Parse(x.Value).RootElement.GetProperty("type").GetRawText();
                                var contentItem = (ContentItem)JsonSerializer.Deserialize(x.Value, config.CurrentValue.ContentItemTypes[(tool.ClientId, type)])!;

                                contentItem.Id = Guid.NewGuid().ToString();
                                contentItem.DeploymentId = deployment.Id;
                                contentItem.ContextId = contextId;

                                return contentItem;
                            })
                            .ToList()
                    };

                    if (!string.IsNullOrWhiteSpace(response.Log))
                    {
                        logger.LogInformation(response.Log);
                    }

                    if (!string.IsNullOrWhiteSpace(response.ErrorLog))
                    {
                        logger.LogError(response.ErrorLog);
                    }

                    if (config.CurrentValue.DeepLink.AutoCreate == true)
                    {
                        await dataService.SaveContentItemsAsync(response.ContentItems);
                    }

                    if (config.CurrentValue.DeepLink.AcceptLineItem == true)
                    {
                        var saveTasks = response.ContentItems
                            .OfType<LtiResourceLinkContentItem>()
                            .Where(i => i.LineItem != null)
                            .Select(i => dataService.SaveLineItemAsync(new LineItem
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = i.LineItem!.Label ?? i.Title ?? i.Type,
                                ScoreMaximum = i.LineItem.ScoreMaximum,
                                GradesReleased = i.LineItem.GradesReleased,
                                Tag = i.LineItem.Tag,
                                ResourceId = i.LineItem.ResourceId,
                                ResourceLinkId = i.Id,
                                StartDateTime = i.Submission?.StartDateTime,
                                EndDateTime = i.Submission?.EndDateTime
                            }));

                        await Task.WhenAll(saveTasks);
                    }

                    return await deepLinkContentHandler.HandleAsync(response);
                })
                .WithName(RouteNames.DEEP_LINKING_RESPONSE)
                .DisableAntiforgery();

            app.MapPost(config.TokenUrl,
                async ([FromForm] TokenRequest request, IDataService dataService, IOptionsMonitor<Lti13PlatformConfig> config) =>
                {
                    const string AUTH_SPEC_URI = "https://www.imsglobal.org/spec/security/v1p0/#using-json-web-tokens-with-oauth-2-0-client-credentials-grant";
                    const string SCOPE_SPEC_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0";
                    const string TOKEN_SPEC_URI = "https://www.imsglobal.org/spec/lti/v1p3/#token-endpoint-claim-and-services";
                    const string UNSUPPORTED_GRANT_TYPE = "unsupported_grant_type";
                    const string INVALID_GRANT = "invalid_grant";
                    const string CLIENT_CREDENTIALS = "client_credentials";
                    const string GRANT_REQUIRED = "grant_type must be 'client_credentials'";
                    const string CLIENT_ASSERTION_TYPE = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
                    const string CLIENT_ASSERTION_TYPE_REQUIRED = "client_assertion_type must be 'urn:ietf:params:oauth:client-assertion-type:jwt-bearer'";
                    const string INVALID_SCOPE = "invalid_scope";
                    const string SCOPE_REQUIRED = "scope must be a valid value";
                    const string CLIENT_ASSERTION_INVALID = "client_assertion must be a valid jwt";
                    const string INVALID_REQUEST = "invalid_request";
                    const string JTI_REUSE = "jti has already been used and is not expired";
                    const string BODY_MISSING = "request body is missing";

                    HashSet<string> SCOPES = [Lti13ServiceScopes.LineItem, Lti13ServiceScopes.LineItemReadOnly, Lti13ServiceScopes.ResultReadOnly, Lti13ServiceScopes.Score];

                    if (request == null)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = BODY_MISSING, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (request.Grant_Type != CLIENT_CREDENTIALS)
                    {
                        return Results.BadRequest(new { Error = UNSUPPORTED_GRANT_TYPE, Error_Description = GRANT_REQUIRED, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (request.Client_Assertion_Type != CLIENT_ASSERTION_TYPE)
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_TYPE_REQUIRED, Error_Uri = AUTH_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Scope))
                    {
                        return Results.BadRequest(new { Error = INVALID_SCOPE, Error_Description = SCOPE_REQUIRED, Error_Uri = SCOPE_SPEC_URI });
                    }

                    var scopes = HttpUtility.UrlDecode(request.Scope).Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .Intersect(SCOPES);

                    if (!scopes.Any())
                    {
                        return Results.BadRequest(new { Error = INVALID_SCOPE, Error_Description = SCOPE_REQUIRED, Error_Uri = SCOPE_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Client_Assertion))
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = AUTH_SPEC_URI });
                    }

                    var jwt = new JsonWebToken(request.Client_Assertion);

                    if (jwt.Issuer != jwt.Subject)
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = TOKEN_SPEC_URI });
                    }

                    var tool = await dataService.GetToolAsync(jwt.Issuer);
                    if (tool?.Jwks == null)
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = TOKEN_SPEC_URI });
                    }

                    var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(request.Client_Assertion, new TokenValidationParameters
                    {
                        IssuerSigningKeys = await tool.Jwks.GetKeysAsync(),
                        ValidAudience = config.CurrentValue.TokenAudience,
                        ValidIssuer = tool.ClientId.ToString()
                    });

                    if (!validatedToken.IsValid)
                    {
                        return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = validatedToken.Exception.Message, Error_Uri = AUTH_SPEC_URI });
                    }
                    else
                    {
                        var serviceToken = await dataService.GetServiceTokenRequestAsync(validatedToken.SecurityToken.Id);
                        if (serviceToken?.Expiration < DateTime.UtcNow)
                        {
                            return Results.BadRequest(new { Error = INVALID_REQUEST, Error_Description = JTI_REUSE, Error_Uri = AUTH_SPEC_URI });
                        }

                        await dataService.SaveServiceTokenRequestAsync(new ServiceToken { Id = validatedToken.SecurityToken.Id, Expiration = validatedToken.SecurityToken.ValidTo });
                    }

                    var privateKey = await dataService.GetPrivateKeyAsync();

                    var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
                    {
                        Subject = validatedToken.ClaimsIdentity,
                        Issuer = config.CurrentValue.Issuer,
                        Audience = config.CurrentValue.Issuer,
                        Expires = DateTime.UtcNow.AddSeconds(config.CurrentValue.AccessTokenExpirationSeconds),
                        SigningCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256),
                        Claims = new Dictionary<string, object>
                        {
                            { ClaimTypes.Role, scopes }
                        }
                    });

                    return Results.Ok(new
                    {
                        access_token = token,
                        token_type = "bearer",
                        expires_in = config.CurrentValue.AccessTokenExpirationSeconds * 60,
                        scope = string.Join(' ', scopes)
                    });
                })
                .DisableAntiforgery();

            app.MapGet(config.AssignmentAndGradeServiceLineItemsUrl,
                async (HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, string contextId, string? resource_id, string? resource_link_id, string? tag, int? limit, int pageIndex = 0) =>
                {
                    var context = await dataService.GetContextAsync(contextId);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItemsResponse = await dataService.GetLineItemsAsync(contextId, pageIndex, limit ?? int.MaxValue, resource_id, resource_link_id, tag);

                    if (lineItemsResponse.TotalItems > 0)
                    {
                        var links = new Collection<string>();
                        if (pageIndex > 0)
                        {
                            links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEMS, new { contextId, resource_id, resource_link_id, tag, limit, pageIndex = pageIndex - 1 })}>; rel=\"prev\"");
                        }

                        if (lineItemsResponse.TotalItems > limit * (pageIndex + 1))
                        {
                            links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEMS, new { contextId, resource_id, resource_link_id, tag, limit, pageIndex = pageIndex + 1 })}>; rel=\"next\"");
                        }

                        links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEMS, new { contextId, resource_id, resource_link_id, tag, limit, pageIndex = 0 })}>; rel=\"first\"");

                        if (limit.HasValue)
                        {
                            links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEMS, new { contextId, resource_id, resource_link_id, tag, limit, pageIndex = Math.Ceiling(lineItemsResponse.TotalItems * 1.0 / limit.GetValueOrDefault()) - 1 })}>; rel=\"last\"");
                        }

                        httpContext.Response.Headers.Link = new StringValues([..links]);
                    }

                    return Results.Json(lineItemsResponse.Items.Select(i => new
                    {
                        Id = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { contextId, i.Id }),
                        i.StartDateTime,
                        i.EndDateTime,
                        i.ScoreMaximum,
                        i.Label,
                        i.Tag,
                        i.ResourceId,
                        i.ResourceLinkId
                    }), contentType: Lti13ContentTypes.LineItemContainer);
                })
                .WithName(RouteNames.GET_LINE_ITEMS)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.LineItem, Lti13ServiceScopes.LineItemReadOnly);
                });

            app.MapPost(config.AssignmentAndGradeServiceLineItemsUrl,
                async (HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, string contextId, LineItemRequest request) =>
                {
                    const string INVALID_CONTENT_TYPE = "Invalid Content-Type";
                    const string CONTENT_TYPE_REQUIRED = "Content-Type must be 'application/vnd.ims.lis.v2.lineitem+json'";
                    const string CONTENT_TYPE_SPEC_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0/#creating-a-new-line-item";
                    const string INVALID_LABEL = "Invalid Label";
                    const string LABEL_REQUIRED = "Label is reuired";
                    const string LABEL_SPEC_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0/#label";
                    const string INVALID_SCORE_MAXIMUM = "Invalid ScoreMaximum";
                    const string SCORE_MAXIMUM_REQUIRED = "ScoreMaximum must be greater than 0";
                    const string SCORE_MAXIUMUM_SPEC_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0/#scoremaximum";

                    var context = await dataService.GetContextAsync(contextId);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    if (httpContext.Request.ContentType != Lti13ContentTypes.LineItem)
                    {
                        return Results.BadRequest(new { Error = INVALID_CONTENT_TYPE, Error_Description = CONTENT_TYPE_REQUIRED, Error_Uri = CONTENT_TYPE_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Label))
                    {
                        return Results.BadRequest(new { Error = INVALID_LABEL, Error_Description = LABEL_REQUIRED, Error_Uri = LABEL_SPEC_URI });
                    }

                    if (request.ScoreMaximum <= 0)
                    {
                        return Results.BadRequest(new { Error = INVALID_SCORE_MAXIMUM, Error_Description = SCORE_MAXIMUM_REQUIRED, Error_Uri = SCORE_MAXIUMUM_SPEC_URI });
                    }

                    if (!string.IsNullOrWhiteSpace(request.ResourceLinkId))
                    {
                        var resourceLink = await dataService.GetContentItemAsync<LtiResourceLinkContentItem>(request.ResourceLinkId);
                        if (resourceLink?.ContextId != contextId)
                        {
                            return Results.NotFound();
                        }
                    }

                    var lineItemId = Guid.NewGuid().ToString();
                    await dataService.SaveLineItemAsync(new LineItem
                    {
                        Id = lineItemId,
                        Label = request.Label,
                        ResourceId = request.ResourceId.ToNullIfEmpty(),
                        ResourceLinkId = request.ResourceLinkId,
                        ScoreMaximum = request.ScoreMaximum,
                        Tag = request.Tag.ToNullIfEmpty(),
                        GradesReleased = request.GradesReleased,
                        StartDateTime = request.StartDateTime,
                        EndDateTime = request.EndDateTime,
                    });

                    var url = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { contextId, lineItemId });
                    return Results.Created(url, new
                    {
                        Id = url,
                        request.Label,
                        request.ResourceId,
                        request.ResourceLinkId,
                        request.ScoreMaximum,
                        request.Tag,
                        request.GradesReleased,
                        request.StartDateTime,
                        request.EndDateTime,
                    });
                })
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.LineItem);
                })
                .DisableAntiforgery();

            app.MapGet(config.AssignmentAndGradeServiceLineItemBaseUrl,
                async (HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, string contextId, string lineItemId) =>
                {
                    var context = await dataService.GetContextAsync(contextId);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItem = await dataService.GetLineItemAsync(lineItemId);
                    if (lineItem == null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Json(new
                    {
                        Id = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { contextId, lineItemId }),
                        lineItem.Label,
                        lineItem.ResourceId,
                        lineItem.ResourceLinkId,
                        lineItem.ScoreMaximum,
                        lineItem.Tag,
                        lineItem.StartDateTime,
                        lineItem.EndDateTime,
                    }, contentType: Lti13ContentTypes.LineItem);
                })
                .WithName(RouteNames.GET_LINE_ITEM)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.LineItem, Lti13ServiceScopes.LineItemReadOnly);
                });

            app.MapPut(config.AssignmentAndGradeServiceLineItemBaseUrl,
                async (HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, string contextId, string lineItemId, LineItemRequest request) =>
                {
                    const string INVALID_CONTENT_TYPE = "Invalid Content-Type";
                    const string CONTENT_TYPE_REQUIRED = "Content-Type must be 'application/vnd.ims.lis.v2.lineitem+json'";
                    const string CONTENT_TYPE_SPEC_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0/#creating-a-new-line-item";
                    const string INVALID_LABEL = "Invalid Label";
                    const string LABEL_REQUIRED = "Label is reuired";
                    const string LABEL_SPEC_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0/#label";
                    const string INVALID_SCORE_MAXIMUM = "Invalid ScoreMaximum";
                    const string SCORE_MAXIMUM_REQUIRED = "ScoreMaximum must be greater than 0";
                    const string SCORE_MAXIUMUM_SPEC_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0/#scoremaximum";
                    const string INVALID_RESOURCE_LINK_ID = "Invalid ResourceLinkId";
                    const string RESOURCE_LINK_ID_MODIFIED = "ResourceLinkId may not change after creation";
                    const string LINE_ITEM_UPDATE_SPEC_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0/#updating-a-line-item";

                    var context = await dataService.GetContextAsync(contextId);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItem = await dataService.GetLineItemAsync(lineItemId);
                    if (lineItem == null)
                    {
                        return Results.NotFound();
                    }

                    if (httpContext.Request.ContentType != Lti13ContentTypes.LineItem)
                    {
                        return Results.BadRequest(new { Error = INVALID_CONTENT_TYPE, Error_Description = CONTENT_TYPE_REQUIRED, Error_Uri = CONTENT_TYPE_SPEC_URI });
                    }

                    if (string.IsNullOrWhiteSpace(request.Label))
                    {
                        return Results.BadRequest(new { Error = INVALID_LABEL, Error_Description = LABEL_REQUIRED, Error_Uri = LABEL_SPEC_URI });
                    }

                    if (request.ScoreMaximum <= 0)
                    {
                        return Results.BadRequest(new { Error = INVALID_SCORE_MAXIMUM, Error_Description = SCORE_MAXIMUM_REQUIRED, Error_Uri = SCORE_MAXIUMUM_SPEC_URI });
                    }

                    if (!string.IsNullOrWhiteSpace(request.ResourceLinkId) && request.ResourceLinkId != lineItem.ResourceLinkId)
                    {
                        return Results.BadRequest(new { Error = INVALID_RESOURCE_LINK_ID, Error_Description = RESOURCE_LINK_ID_MODIFIED, Error_Uri = LINE_ITEM_UPDATE_SPEC_URI });
                    }

                    lineItem.Label = request.Label;
                    lineItem.ResourceId = request.ResourceId.ToNullIfEmpty();
                    lineItem.ResourceLinkId = request.ResourceLinkId;
                    lineItem.ScoreMaximum = request.ScoreMaximum;
                    lineItem.Tag = request.Tag.ToNullIfEmpty();
                    lineItem.GradesReleased = request.GradesReleased;
                    lineItem.StartDateTime = request.StartDateTime;
                    lineItem.EndDateTime = request.EndDateTime;

                    await dataService.SaveLineItemAsync(lineItem);

                    return Results.Json(new
                    {
                        Id = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { contextId, lineItemId }),
                        lineItem.Label,
                        lineItem.ResourceId,
                        lineItem.ResourceLinkId,
                        lineItem.ScoreMaximum,
                        lineItem.Tag,
                        lineItem.GradesReleased,
                        lineItem.StartDateTime,
                        lineItem.EndDateTime,
                    }, contentType: Lti13ContentTypes.LineItem);
                })
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.LineItem);
                })
                .DisableAntiforgery();

            app.MapDelete(config.AssignmentAndGradeServiceLineItemBaseUrl,
                async (IDataService dataService, string contextId, string lineItemId) =>
                {
                    var context = await dataService.GetContextAsync(contextId);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItem = await dataService.GetLineItemAsync(lineItemId);
                    if (lineItem == null)
                    {
                        return Results.NotFound();
                    }

                    await dataService.DeleteLineItemAsync(lineItemId);

                    return Results.NoContent();
                })
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.LineItem);
                })
                .DisableAntiforgery();

            app.MapGet($"{config.AssignmentAndGradeServiceLineItemBaseUrl}/results",
                async (HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, string contextId, string lineItemId, string? user_id, int? limit, int pageIndex = 0) =>
                {
                    var context = await dataService.GetContextAsync(contextId);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItem = await dataService.GetLineItemAsync(lineItemId);
                    if (lineItem == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItemsResponse = await dataService.GetLineItemResultsAsync(contextId, lineItemId, pageIndex, limit ?? int.MaxValue, user_id);

                    if (lineItemsResponse.TotalItems > 0)
                    {
                        var links = new Collection<string>();
                        if (pageIndex > 0)
                        {
                            links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM_RESULTS, new { contextId, lineItemId, limit, pageIndex = pageIndex - 1 })}>; rel=\"prev\"");
                        }

                        if (lineItemsResponse.TotalItems > limit * (pageIndex + 1))
                        {
                            links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM_RESULTS, new { contextId, lineItemId, limit, pageIndex = pageIndex + 1 })}>; rel=\"next\"");
                        }
                        links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM_RESULTS, new { contextId, lineItemId, limit, pageIndex = 0 })}>; rel=\"first\"");
                        if (limit.HasValue)
                        {
                            links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM_RESULTS, new { contextId, lineItemId, limit, pageIndex = Math.Ceiling(lineItemsResponse.TotalItems * 1.0 / limit.GetValueOrDefault()) - 1 })}>; rel=\"last\"");
                        }
                        httpContext.Response.Headers.Link = new StringValues([..links]);
                    }

                    return Results.Json(lineItemsResponse.Items.Select(i => new
                    {
                        Id = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM_RESULTS, new { contextId, i.LineItemId, user_id = i.UserId }),
                        ScoreOf = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { contextId, i.LineItemId }),
                        i.UserId,
                        i.ResultScore,
                        i.ResultMaximum,
                        i.ScoringUserId,
                        i.Comment
                    }), contentType: Lti13ContentTypes.ResultContainer);
                })
                .WithName(RouteNames.GET_LINE_ITEM_RESULTS)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.ResultReadOnly);
                });

            app.MapPost($"{config.AssignmentAndGradeServiceLineItemBaseUrl}/scores",
                async (IDataService dataService, string contextId, string lineItemId, ScoreRequest request) =>
                {
                    const string RESULT_TOO_EARLY = "startDateTime";
                    const string RESULT_TOO_EARLY_DESCRIPTION = "lineItem startDateTime is in the future";
                    const string RESULT_TOO_EARLY_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0/#startdatetime";
                    const string RESULT_TOO_LATE = "endDateTime";
                    const string RESULT_TOO_LATE_DESCRIPTION = "lineItem endDateTime is in the past";
                    const string RESULT_TOO_LATE_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0/#enddatetime";
                    const string OUT_OF_DATE = "timestamp";
                    const string OUT_OF_DATE_DESCRIPTION = "timestamp must be after the current timestamp";
                    const string OUT_OF_DATE_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0/#timestamp";

                    var context = await dataService.GetContextAsync(contextId);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItem = await dataService.GetLineItemAsync(lineItemId);
                    if (lineItem == null)
                    {
                        return Results.NotFound();
                    }

                    if (DateTime.UtcNow < lineItem.StartDateTime)
                    {
                        return Results.Json(new
                        {
                            Error = RESULT_TOO_EARLY,
                            Error_Description = RESULT_TOO_EARLY_DESCRIPTION,
                            Error_Uri = RESULT_TOO_EARLY_URI
                        }, statusCode: (int)HttpStatusCode.Forbidden);
                    }

                    if (DateTime.UtcNow > lineItem.EndDateTime)
                    {
                        return Results.Json(new
                        {
                            Error = RESULT_TOO_LATE,
                            Error_Description = RESULT_TOO_LATE_DESCRIPTION,
                            Error_Uri = RESULT_TOO_LATE_URI
                        }, statusCode: (int)HttpStatusCode.Forbidden);
                    }

                    var isNew = false;
                    var result = (await dataService.GetLineItemResultsAsync(contextId, lineItemId, 0, 1, request.UserId)).Items.FirstOrDefault();
                    if (result == null)
                    {
                        isNew = true;
                        result = new Result
                        {
                            LineItemId = lineItemId,
                            UserId = request.UserId
                        };
                    }
                    else if (result.Timestamp >= request.TimeStamp)
                    {
                        return Results.Conflict(new
                        {
                            Error = OUT_OF_DATE,
                            Error_Description = OUT_OF_DATE_DESCRIPTION,
                            Error_Uri = OUT_OF_DATE_URI
                        });
                    }

                    result.ResultScore = request.ScoreGiven;
                    result.ResultMaximum = request.ScoreMaximum;
                    result.Comment = request.Comment;
                    result.ScoringUserId = request.ScoringUserId;
                    result.Timestamp = request.TimeStamp;

                    await dataService.SaveLineItemResultAsync(result);

                    return isNew ? Results.Created() : Results.NoContent();
                })
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.Score);
                })
                .DisableAntiforgery();

            return app;
        }
    }

    internal record DeepLinkResponseRequest(string? Jwt);

    public record AuthenticationRequest(string? Scope, string? Response_Type, string? Response_Mode, string? Prompt, string? Nonce, string? State, string? Client_Id, string? Redirect_Uri, string? Login_Hint, string? Lti_Message_Hint);

    internal record LineItemRequest(decimal ScoreMaximum, string Label, string? ResourceLinkId, string? ResourceId, string? Tag, bool? GradesReleased, DateTime? StartDateTime, DateTime? EndDateTime);

    internal record LineItemPutRequest(DateTime StartDateTime, DateTime EndDateTime, decimal ScoreMaximum, string Label, string Tag, string ResourceId, string ResourceLinkId);

    internal record LineItemsPostRequest(DateTime StartDateTime, DateTime EndDateTime, decimal ScoreMaximum, string Label, string Tag, string ResourceId, string ResourceLinkId, bool? GradesReleased);

    internal record ScoreRequest(string UserId, string ScoringUserId, decimal ScoreGiven, decimal ScoreMaximum, string Comment, ScoreSubmissionRequest? Submision, DateTime TimeStamp, ActivityProgress ActivityProgress, GradingProgress GradingProgress);

    internal record ScoreSubmissionRequest(DateTime? StartedAt, DateTime? SubmittedAt);

    internal record TokenRequest(string Grant_Type, string Client_Assertion_Type, string Client_Assertion, string Scope);

    internal static class Lti13ContentTypes
    {
        internal const string LineItemContainer = "application/vnd.ims.lis.v2.lineitemcontainer+json";
        internal const string LineItem = "application/vnd.ims.lis.v2.lineitem+json";
        internal const string ResultContainer = "application/vnd.ims.lis.v2.resultcontainer+json";
        internal const string Score = "application/vnd.ims.lis.v1.score+json";
    }

    internal class LtiServicesAuthHandler(IDataService dataService, IOptionsMonitor<Lti13PlatformConfig> config, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) :
        AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "NP.Lti13Platform.Services";

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //// TODO: Testing only
            //return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity([
            //    new Claim(ClaimTypes.Role, Lti13ServiceScopes.Score),
            //    new Claim(ClaimTypes.Role, Lti13ServiceScopes.ResultReadOnly),
            //    new Claim(ClaimTypes.Role, Lti13ServiceScopes.LineItem),
            //    new Claim(ClaimTypes.Role, Lti13ServiceScopes.LineItemReadOnly)
            //    ])), SchemeName));

            var authHeaderParts = Context.Request.Headers.Authorization.ToString().Trim().Split(' ');

            if (authHeaderParts.Length != 2 || authHeaderParts[0] != "Bearer")
            {
                return AuthenticateResult.NoResult();
            }

            var publicKeys = await dataService.GetPublicKeysAsync();

            var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(authHeaderParts[1], new TokenValidationParameters
            {
                IssuerSigningKeys = publicKeys,
                ValidAudience = config.CurrentValue.Issuer,
                ValidIssuer = config.CurrentValue.Issuer
            });

            return validatedToken.IsValid ? AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal([validatedToken.ClaimsIdentity]), SchemeName)) : AuthenticateResult.NoResult();
        }
    }

    public static class Lti13ServiceScopes
    {
        public const string LineItem = "https://purl.imsglobal.org/spec/lti-ags/scope/lineitem";
        public const string LineItemReadOnly = "https://purl.imsglobal.org/spec/lti-ags/scope/lineitem.readonly";
        public const string ResultReadOnly = "https://purl.imsglobal.org/spec/lti-ags/scope/result.readonly";
        public const string Score = "https://purl.imsglobal.org/spec/lti-ags/scope/score";
    }

    public enum ActivityProgress
    {
        Initialized,
        Started,
        InProgress,
        Submitted,
        Completed
    }

    public enum GradingProgress
    {
        FullyGraded,
        Pending,
        PendingManual,
        Failed,
        NotReady
    }
}
