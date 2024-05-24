using Microsoft.AspNetCore.Authentication;
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
        public static string GET_LINE_ITEMS = "GET_LINE_ITEMS";
        public static string GET_LINE_ITEM = "GET_LINE_ITEM";
        public static string GET_LINE_ITEM_RESULTS = "GET_LINE_ITEM_RESULTS";
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

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, LtiServicesAuthHandler>(LtiServicesAuthHandler.SchemeName, null);

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
                async ([AsParameters] AuthenticationRequest request, LinkGenerator linkGenerator, HttpContext httpContext, IOptionsMonitor<Lti13PlatformConfig> config, Service service, IDataService dataService) => await AuthenticationHandler.HandleAsync(linkGenerator, httpContext, config, service, dataService, request));

            app.MapPost(config.AuthorizationUrl,
                async ([FromForm] AuthenticationRequest request, LinkGenerator linkGenerator, HttpContext httpContext, IOptionsMonitor<Lti13PlatformConfig> config, Service service, IDataService dataService) => await AuthenticationHandler.HandleAsync(linkGenerator, httpContext, config, service, dataService, request))
                .DisableAntiforgery();

            app.MapPost(config.DeepLinkResponseUrl,
                async ([FromForm] DeepLinkResponseRequest request, IDataService dataService, IOptionsMonitor<Lti13PlatformConfig> config, IServiceProvider serviceProvider) =>
                {
                    if (string.IsNullOrWhiteSpace(request.Jwt))
                    {
                        return Results.BadRequest("NO JWT FOUND");
                    }

                    var jwt = new JsonWebToken(request.Jwt);

                    if (!jwt.TryGetClaim("https://purl.imsglobal.org/spec/lti/claim/deployment_id", out var deploymentIdClaim) || Guid.TryParse(deploymentIdClaim.Value, out var deploymentId))
                    {
                        return Results.BadRequest("BAD DEPLOYMENT ID");
                    }

                    var deployment = await dataService.GetDeploymentAsync(deploymentId);
                    if (deployment == null)
                    {
                        return Results.BadRequest("BAD DEPLOYMENT ID");
                    }

                    var client = await dataService.GetClientAsync(deployment.ClientId);
                    if (client?.Jwks == null)
                    {
                        return Results.BadRequest("BAD DEPLOYMENT ID");
                    }

                    var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(request.Jwt, new TokenValidationParameters
                    {
                        IssuerSigningKeys = await client.Jwks.GetKeysAsync(),
                        ValidAudience = config.CurrentValue.Issuer,
                        ValidIssuer = client.Id.ToString()
                    });

                    if (!validatedToken.IsValid)
                    {
                        return Results.BadRequest(validatedToken.Exception);
                    }

                    if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/message_type", out var messageType) || (string)messageType != "LtiDeepLinkingResponse")
                    {
                        return Results.BadRequest("BAD MESSAGE TYPE");
                    }

                    if (!validatedToken.Claims.TryGetValue("https://purl.imsglobal.org/spec/lti/claim/version", out var version) || (string)version != "1.3.0")
                    {
                        return Results.BadRequest("BAD VERSION");
                    }

                    var dataParts = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/data")?.Value.Split('|', 2) ?? [string.Empty, string.Empty];

                    var response = new DeepLinkResponse
                    {
                        Data = dataParts[1],
                        Message = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/msg")?.Value,
                        Log = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/log")?.Value,
                        ErrorMessage = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errormsg")?.Value,
                        ErrorLog = validatedToken.ClaimsIdentity.FindFirst("https://purl.imsglobal.org/spec/lti-dl/claim/errorlog")?.Value,
                        ContentItems = validatedToken.ClaimsIdentity.FindAll("https://purl.imsglobal.org/spec/lti-dl/claim/content_items")
                            .Select(x =>
                            {
                                var document = JsonDocument.Parse(x.Value);
                                var property = document.RootElement.GetProperty("type");
                                var type = property.GetRawText();
                                var contentItem = (ContentItem)JsonSerializer.Deserialize(x.Value, config.CurrentValue.ContentItemTypes[(client.Id, type)])!;

                                contentItem.Id = Guid.NewGuid();
                                contentItem.DeploymentId = deploymentId;
                                contentItem.ContextId = Guid.TryParse(dataParts[0], out var contextId) ? contextId : null;

                                return contentItem;
                            })
                    };

                    // TODO: Figure out logger with minimal api
                    //if (!string.IsNullOrWhiteSpace(response.Log))
                    //{
                    //    logger.LogInformation(response.Log);
                    //}

                    //if (!string.IsNullOrWhiteSpace(response.ErrorLog))
                    //{
                    //    logger.LogError(response.ErrorLog);
                    //}

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
                                Id = Guid.NewGuid(),
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

                    return await serviceProvider.GetRequiredService<IDeepLinkContentHandler>().HandleAsync(response);
                })
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

                    IEnumerable<string> scopes;
                    if (string.IsNullOrWhiteSpace(request.Scope))
                    {
                        return Results.BadRequest(new { Error = INVALID_SCOPE, Error_Description = SCOPE_REQUIRED, Error_Uri = SCOPE_SPEC_URI });
                    }
                    else
                    {
                        scopes = HttpUtility.UrlDecode(request.Scope).Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                            .Intersect(SCOPES);

                        if (!scopes.Any())
                        {
                            return Results.BadRequest(new { Error = INVALID_SCOPE, Error_Description = SCOPE_REQUIRED, Error_Uri = SCOPE_SPEC_URI });
                        }
                    }

                    if (string.IsNullOrWhiteSpace(request.Client_Assertion))
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = AUTH_SPEC_URI });
                    }

                    var jwt = new JsonWebToken(request.Client_Assertion);

                    Client? client;
                    if (jwt.Issuer != jwt.Subject || !Guid.TryParse(jwt.Issuer, out var issuer))
                    {
                        return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = TOKEN_SPEC_URI });
                    }
                    else
                    {
                        client = await dataService.GetClientAsync(issuer);
                        if (client?.Jwks == null)
                        {
                            return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = TOKEN_SPEC_URI });
                        }
                    }

                    var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(request.Client_Assertion, new TokenValidationParameters
                    {
                        IssuerSigningKeys = await client.Jwks.GetKeysAsync(),
                        ValidAudience = config.CurrentValue.TokenAudience,
                        ValidIssuer = client.Id.ToString()
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
                async (HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, Guid contextId, string? resource_id, Guid? resource_link_id, string? tag, int? limit, int pageIndex = 0) =>
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

                        httpContext.Response.Headers.Link = new StringValues(links.ToArray());
                    }

                    return Results.Json(lineItemsResponse.Items.Select(i => new
                    {
                        Id = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { contextId, i.Id }),
                        StartDateTime = i.StartDateTime,
                        EndDateTime = i.EndDateTime,
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
                async (HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, Guid contextId, LineItemRequest request) =>
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
                        return Results.BadRequest(new
                        {
                            Error = INVALID_CONTENT_TYPE,
                            Error_Description = CONTENT_TYPE_REQUIRED,
                            Error_Uri = CONTENT_TYPE_SPEC_URI
                        });
                    }

                    if (string.IsNullOrWhiteSpace(request.Label))
                    {
                        return Results.BadRequest(new
                        {
                            Error = INVALID_LABEL,
                            Error_Description = LABEL_REQUIRED,
                            Error_Uri = LABEL_SPEC_URI
                        });
                    }

                    if (request.ScoreMaximum <= 0)
                    {
                        return Results.BadRequest(new
                        {
                            Error = INVALID_SCORE_MAXIMUM,
                            Error_Description = SCORE_MAXIMUM_REQUIRED,
                            Error_Uri = SCORE_MAXIUMUM_SPEC_URI
                        });
                    }

                    if (request.ResourceLinkId.HasValue)
                    {
                        var resourceLink = await dataService.GetContentItemAsync<LtiResourceLinkContentItem>(request.ResourceLinkId.GetValueOrDefault());
                        if (resourceLink?.ContextId != contextId)
                        {
                            return Results.NotFound();
                        }
                    }

                    var lineItemId = Guid.NewGuid();
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
                async (HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, Guid contextId, Guid lineItemId) =>
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
                        StartDateTime = lineItem.StartDateTime,
                        EndDateTime = lineItem.EndDateTime,
                    }, contentType: Lti13ContentTypes.LineItem);
                })
                .WithName(RouteNames.GET_LINE_ITEM)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.LineItem, Lti13ServiceScopes.LineItemReadOnly);
                });

            app.MapPut(config.AssignmentAndGradeServiceLineItemBaseUrl,
                async (HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, Guid contextId, Guid lineItemId, LineItemRequest request) =>
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
                        return Results.BadRequest(new
                        {
                            Error = INVALID_CONTENT_TYPE,
                            Error_Description = CONTENT_TYPE_REQUIRED,
                            Error_Uri = CONTENT_TYPE_SPEC_URI
                        });
                    }

                    if (string.IsNullOrWhiteSpace(request.Label))
                    {
                        return Results.BadRequest(new
                        {
                            Error = INVALID_LABEL,
                            Error_Description = LABEL_REQUIRED,
                            Error_Uri = LABEL_SPEC_URI
                        });
                    }

                    if (request.ScoreMaximum <= 0)
                    {
                        return Results.BadRequest(new
                        {
                            Error = INVALID_SCORE_MAXIMUM,
                            Error_Description = SCORE_MAXIMUM_REQUIRED,
                            Error_Uri = SCORE_MAXIUMUM_SPEC_URI
                        });
                    }

                    if (request.ResourceLinkId.HasValue && request.ResourceLinkId != lineItem.ResourceLinkId)
                    {
                        return Results.BadRequest(new
                        {
                            Error = INVALID_RESOURCE_LINK_ID,
                            Error_Description = RESOURCE_LINK_ID_MODIFIED,
                            Error_Uri = LINE_ITEM_UPDATE_SPEC_URI
                        });
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
                        StartDateTime = lineItem.StartDateTime,
                        EndDateTime = lineItem.EndDateTime,
                    }, contentType: Lti13ContentTypes.LineItem);
                })
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.LineItem);
                })
                .DisableAntiforgery();

            app.MapDelete(config.AssignmentAndGradeServiceLineItemBaseUrl,
                async (IDataService dataService, Guid contextId, Guid lineItemId) =>
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
                async (HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, Guid contextId, Guid lineItemId, string? user_id, int? limit, int pageIndex = 0) =>
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
                        httpContext.Response.Headers.Link = new StringValues(links.ToArray());
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
                async (IDataService dataService, Guid contextId, Guid lineItemId, ScoreRequest request) =>
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
                            Id = Guid.NewGuid(),
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

    internal record LineItemRequest(decimal ScoreMaximum, string Label, Guid? ResourceLinkId, string? ResourceId, string? Tag, bool? GradesReleased, DateTime? StartDateTime, DateTime? EndDateTime);

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
        public static string SchemeName = "NP.Lti13Platform.Services";

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

    public class TokenRequest
    {
        public string Grant_Type { get; set; }
        public string Client_Assertion_Type { get; set; }
        public string Client_Assertion { get; set; }
        public string Scope { get; set; }
    }

    public static class Lti13ServiceScopes
    {
        public static readonly string LineItem = "https://purl.imsglobal.org/spec/lti-ags/scope/lineitem";
        public static readonly string LineItemReadOnly = "https://purl.imsglobal.org/spec/lti-ags/scope/lineitem.readonly";
        public static readonly string ResultReadOnly = "https://purl.imsglobal.org/spec/lti-ags/scope/result.readonly";
        public static readonly string Score = "https://purl.imsglobal.org/spec/lti-ags/scope/score";
    }

    public class ServiceToken
    {
        public required string Id { get; set; }
        public required DateTime Expiration { get; set; }
    }

    public class LineItemsPostRequest // same as LineItemPutRequest // same as LineItemResponse (minus id)
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal ScoreMaximum { get; set; }
        public string Label { get; set; }
        public string Tag { get; set; }
        public string ResourceId { get; set; }
        public string ResourceLinkId { get; set; }
        public bool? GradesReleased { get; set; }
    }

    public class LineItemPutRequest
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal ScoreMaximum { get; set; }
        public string Label { get; set; }
        public string Tag { get; set; }
        public string ResourceId { get; set; }
        public string ResourceLinkId { get; set; }
    }

    public class ScoreRequest
    {
        public string UserId { get; set; }
        public string ScoringUserId { get; set; }
        public decimal ScoreGiven { get; set; }
        public decimal ScoreMaximum { get; set; }
        public string Comment { get; set; }
        public ScoreSubmissionRequest? Submission { get; set; }
        public DateTime TimeStamp { get; set; }
        public ActivityProgress ActivityProgress { get; set; }
        public GradingProgress GradingProgress { get; set; }

        public class ScoreSubmissionRequest
        {
            public DateTime? StartedAt { get; set; }
            public DateTime? SubmittedAt { get; set; }
        }
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
