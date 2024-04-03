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
using System.Collections.ObjectModel;
using System.Security.Claims;
using System.Text.Encodings.Web;
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
        public static IServiceCollection AddLti13Platform(this IServiceCollection services, Action<Lti13PlatformConfig> configure)
        {
            services.Configure(configure);

            services.AddTransient<Service>();
            services.AddTransient<AuthenticationHandler>();
            services.AddTransient<DeepLinkHandler>();
            services.AddTransient<JwksHandler>();
            services.AddTransient<TokenHandler>();

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, LtiServicesAuthHandler>(LtiServicesAuthHandler.SchemeName, null);

            return services;
        }

        public static IEndpointRouteBuilder UseLti13Platform(this IEndpointRouteBuilder app, Action<Lti13PlatformEndpointsConfig>? configure = null)
        {
            var config = new Lti13PlatformEndpointsConfig();
            configure?.Invoke(config);

            app.MapGet(config.JwksUrl, async (JwksHandler handler) => await handler.HandleAsync());

            app.MapGet(config.AuthorizationUrl, async ([AsParameters] AuthenticationRequest request, AuthenticationHandler handler) => await handler.HandleAsync(request));
            app.MapPost(config.AuthorizationUrl, async ([FromForm] AuthenticationRequest request, AuthenticationHandler handler) => await handler.HandleAsync(request)).DisableAntiforgery();

            app.MapPost(config.DeepLinkResponseUrl, async ([FromForm] DeepLinkResponseRequest request, DeepLinkHandler handler) => await handler.HandleAsync(request)).DisableAntiforgery();

            app.MapPost(config.TokenUrl, async ([FromForm] TokenRequest request, TokenHandler handler) => await handler.HandleAsync(request)).DisableAntiforgery();

            app.MapGet(config.AssignmentAndGradeServiceLineItemsUrl, Lti13Ags.GetLineItemAsync)
                .WithName(RouteNames.GET_LINE_ITEMS)
                .RequireAuthorization(policy =>
                 {
                     policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                     policy.RequireRole(Lti13ServiceScopes.LineItem, Lti13ServiceScopes.LineItemReadOnly);
                 });

            app.MapPost(config.AssignmentAndGradeServiceLineItemsUrl, Lti13Ags.CreateLineItemAsync)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.LineItem);
                });

            app.MapGet(config.AssignmentAndGradeServiceLineItemBaseUrl, Lti13Ags.GetLineItemAsync)
                .WithName(RouteNames.GET_LINE_ITEM)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.LineItem, Lti13ServiceScopes.LineItemReadOnly);
                });

            app.MapPut(config.AssignmentAndGradeServiceLineItemBaseUrl, Lti13Ags.UpdateLineItemAsync)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.LineItem);
                });

            app.MapDelete(config.AssignmentAndGradeServiceLineItemBaseUrl, Lti13Ags.DeleteLineItemAsync)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.LineItem);
                });

            app.MapGet($"{config.AssignmentAndGradeServiceLineItemBaseUrl}/results", Lti13Ags.GetLineItemResultsAsync)
                .WithName(RouteNames.GET_LINE_ITEM_RESULTS)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.ResultReadOnly);
                });

            app.MapPost($"{config.AssignmentAndGradeServiceLineItemBaseUrl}/scores", Lti13Ags.PublishScoreAsync)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(Lti13ServiceScopes.Score);
                });

            return app;
        }
    }

    internal static class Lti13Ags
    {
        internal static async Task<IResult> GetLineItemsAsync(HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, string contextId, string? resource_id, string? resource_link_id, string? tag, int? limit, int pageIndex = 0)
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
                i.StartDateTime,
                i.EndDateTime,
                i.ScoreMaximum,
                i.Label,
                i.Tag,
                i.ResourceId,
                i.ResourceLinkId
            }), contentType: Lti13ContentTypes.LineItemContainer);
        }

        internal static async Task<IResult> CreateLineItemAsync(HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, string contextId, LineItemsPostRequest request)
        {
            const string INVALID_CONTENT_TYPE = "Invalid Content Type";
            const string CONTENT_TYPE_REQUIRED = "Content type must be 'application/vnd.ims.lis.v2.lineitem+json'";
            const string CONTENT_TYPE_SPEC_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0/#creating-a-new-line-item";

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

            var lineItemId = await dataService.SaveLineItemAsync(new LineItem
            {
                Label = request.Label,
                ResourceId = request.ResourceId,
                ResourceLinkId = request.ResourceLinkId,
                ScoreMaximum = request.ScoreMaximum,
                Tag = request.Tag,
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
                request.StartDateTime,
                request.EndDateTime,
            });
        }

        internal static async Task<IResult> GetLineItemAsync(HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, string contextId, string lineItemId)
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
        }

        internal static async Task<IResult> UpdateLineItemAsync(HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, string contextId, string lineItemId, LineItemPutRequest request)
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

            lineItem.Label = request.Label;
            lineItem.ResourceId = request.ResourceId;
            lineItem.ResourceLinkId = request.ResourceLinkId;
            lineItem.ScoreMaximum = request.ScoreMaximum;
            lineItem.Tag = request.Tag;
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
                lineItem.StartDateTime,
                lineItem.EndDateTime,
            }, contentType: Lti13ContentTypes.LineItem);
        }

        internal static async Task<IResult> DeleteLineItemAsync(IDataService dataService, string contextId, string lineItemId)
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
        }

        internal static async Task<IResult> GetLineItemResultsAsync(HttpContext httpContext, IDataService dataService, LinkGenerator linkGenerator, string contextId, string lineItemId, string? user_id, int? limit, int pageIndex = 0)
        {
            var context = await dataService.GetContextAsync(contextId);
            if (context == null)
            {
                return Results.NotFound();
            }

            var lineItem = await dataService.GetLineItemAsync(contextId);
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
                Id = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM_RESULTS, new { contextId, i.Id }), // TODO: make url for a single result (does not need to be an actual endpoint
                ScoreOf = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { contextId, i.LineItemId }),
                i.UserId,
                i.ResultScore,
                i.ResultMaximum,
                i.ScoringUserId,
                i.Comment
            }), contentType: Lti13ContentTypes.ResultContainer);
        }

        internal static async Task<IResult> PublishScoreAsync(IDataService dataService, string contextId, string lineItemId, ScoreRequest request)
        {
            var context = await dataService.GetContextAsync(contextId);
            if (context == null)
            {
                return Results.NotFound();
            }

            var lineItem = await dataService.GetLineItemAsync(contextId);
            if (lineItem == null)
            {
                return Results.NotFound();
            }

            // TODO
            //if (!lineItem.IsAcceptingChanges)
            //{
            //    return Results.Forbid(); // 403 cannot be applied (ie. activity is closed, not accepting changes anymore), describe reason for rejection
            //}

            var isNew = false;
            var result = await dataService.GetLineItemResultAsync(contextId, lineItemId, request.UserId);
            if (result == null)
            {
                isNew = true;
                result = new LineItemResult
                {
                    LineItemId = lineItemId,
                    UserId = request.UserId                    
                };
            }
            else if (result.Timestamp >= request.TimeStamp)
            {
                return Results.Conflict();
            }

            result.ResultScore = request.ScoreGiven;
            result.ResultMaximum = request.ScoreMaximum;
            result.Comment = request.Comment;
            result.ScoringUserId = request.ScoringUserId;
            result.Timestamp = request.TimeStamp;

            await dataService.SaveLineItemResultAsync(result);

            return isNew ? Results.Created() : Results.NoContent();
        }
    }

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

    public class TokenHandler(IDataService dataService, IOptionsMonitor<Lti13PlatformConfig> config)
    {
        private const string AUTH_SPEC_URI = "https://www.imsglobal.org/spec/security/v1p0/#using-json-web-tokens-with-oauth-2-0-client-credentials-grant";
        private const string SCOPE_SPEC_URI = "https://www.imsglobal.org/spec/lti-ags/v2p0";
        private const string TOKEN_SPEC_URI = "https://www.imsglobal.org/spec/lti/v1p3/#token-endpoint-claim-and-services";
        private const string UNSUPPORTED_GRANT_TYPE = "unsupported_grant_type";
        private const string INVALID_GRANT = "invalid_grant";
        private const string CLIENT_CREDENTIALS = "client_credentials";
        private const string GRANT_REQUIRED = "grant_type must be 'client_credentials'";
        private const string CLIENT_ASSERTION_TYPE = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
        private const string CLIENT_ASSERTION_TYPE_REQUIRED = "client_assertion_type must be 'urn:ietf:params:oauth:client-assertion-type:jwt-bearer'";
        private const string INVALID_SCOPE = "invalid_scope";
        private const string SCOPE_REQUIRED = "scope must be a valid value";
        private const string CLIENT_ASSERTION_INVALID = "client_assertion must be a valid jwt";
        private const string INVALID_REQUEST = "invalid_request";
        private const string JTI_REUSE = "jti has already been used and is not expired";
        private const string BODY_MISSING = "request body is missing";

        private readonly HashSet<string> SCOPES = [Lti13ServiceScopes.LineItem, Lti13ServiceScopes.LineItemReadOnly, Lti13ServiceScopes.ResultReadOnly, Lti13ServiceScopes.Score];

        public async Task<IResult> HandleAsync(TokenRequest request)
        {
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
                    .Where(SCOPES.Contains)
                    .ToList();

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

            Lti13Client? client;
            if (jwt.Issuer != jwt.Subject)
            {
                return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = TOKEN_SPEC_URI });
            }
            else
            {
                client = await dataService.GetClientAsync(jwt.Issuer);
                if (client?.Jwks == null)
                {
                    return Results.BadRequest(new { Error = INVALID_GRANT, Error_Description = CLIENT_ASSERTION_INVALID, Error_Uri = TOKEN_SPEC_URI });
                }
            }

            var validatedToken = await new JsonWebTokenHandler().ValidateTokenAsync(request.Client_Assertion, new TokenValidationParameters
            {
                IssuerSigningKeys = await client.Jwks.GetKeysAsync(),
                ValidAudience = config.CurrentValue.TokenAudience,
                ValidIssuer = client.Id
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
        }
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
