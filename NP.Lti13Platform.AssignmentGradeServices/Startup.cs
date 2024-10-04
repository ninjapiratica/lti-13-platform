using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Extensions;
using NP.Lti13Platform.Core.Models;
using System.Collections.ObjectModel;
using System.Net;

namespace NP.Lti13Platform.AssignmentGradeServices
{
    public static class Startup
    {
        public static Lti13PlatformBuilder AddLti13PlatformAssignmentGradeServices(this Lti13PlatformBuilder builder)
        {
            builder.ExtendLti13Message<IServiceEndpoints, AssignmentGradeServiceEndpointsPopulator>();

            return builder;
        }

        public static Lti13PlatformEndpointRouteBuilder UseLti13PlatformAssignmentGradeServices(this Lti13PlatformEndpointRouteBuilder app, Action<Lti13PlatformAGSEndpointsConfig>? configure = null)
        {
            var config = new Lti13PlatformAGSEndpointsConfig();
            configure?.Invoke(config);

            app.MapGet(config.AssignmentAndGradeServiceLineItemsUrl,
                async (IHttpContextAccessor httpContextAccessor, IDataService dataService, LinkGenerator linkGenerator, string contextId, string? resource_id, string? resource_link_id, string? tag, int? limit, int pageIndex = 0) =>
                {
                    var httpContext = httpContextAccessor.HttpContext!;
                    var context = await dataService.GetContextAsync(contextId);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItemsResponse = await dataService.GetLineItemsAsync(contextId, pageIndex, limit ?? int.MaxValue, resource_id, resource_link_id, tag);

                    if (lineItemsResponse.TotalItems > 0 && limit.HasValue)
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

                        links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEMS, new { contextId, resource_id, resource_link_id, tag, limit, pageIndex = Math.Ceiling(lineItemsResponse.TotalItems * 1.0 / limit.GetValueOrDefault()) - 1 })}>; rel=\"last\"");

                        httpContext.Response.Headers.Link = new StringValues([.. links]);
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
                async (IHttpContextAccessor httpContextAccessor, IDataService dataService, LinkGenerator linkGenerator, string contextId, LineItemRequest request) =>
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

                    var httpContext = httpContextAccessor.HttpContext!;
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
                async (IHttpContextAccessor httpContextAccessor, IDataService dataService, LinkGenerator linkGenerator, string contextId, string lineItemId) =>
                {
                    var httpContext = httpContextAccessor.HttpContext!;
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
                async (IHttpContextAccessor httpContextAccessor, IDataService dataService, LinkGenerator linkGenerator, string contextId, string lineItemId, LineItemRequest request) =>
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

                    var httpContext = httpContextAccessor.HttpContext!;
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
                async (IHttpContextAccessor httpContextAccessor, IDataService dataService, LinkGenerator linkGenerator, string contextId, string lineItemId, string? user_id, int? limit, int pageIndex = 0) =>
                {
                    var httpContext = httpContextAccessor.HttpContext!;
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

                    var lineItemsResponse = await dataService.GetGradesAsync(contextId, lineItemId, pageIndex, limit ?? int.MaxValue, user_id);

                    if (lineItemsResponse.TotalItems > 0 && limit.HasValue)
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

                        links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM_RESULTS, new { contextId, lineItemId, limit, pageIndex = Math.Ceiling(lineItemsResponse.TotalItems * 1.0 / limit.GetValueOrDefault()) - 1 })}>; rel=\"last\"");

                        httpContext.Response.Headers.Link = new StringValues([.. links]);
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
                    var result = (await dataService.GetGradesAsync(contextId, lineItemId, 0, 1, request.UserId)).Items.FirstOrDefault();
                    if (result == null)
                    {
                        isNew = true;
                        result = new Grade
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

                    await dataService.SaveGradeAsync(result);

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

    internal record LineItemRequest(decimal ScoreMaximum, string Label, string? ResourceLinkId, string? ResourceId, string? Tag, bool? GradesReleased, DateTime? StartDateTime, DateTime? EndDateTime);

    internal record LineItemPutRequest(DateTime StartDateTime, DateTime EndDateTime, decimal ScoreMaximum, string Label, string Tag, string ResourceId, string ResourceLinkId);

    internal record LineItemsPostRequest(DateTime StartDateTime, DateTime EndDateTime, decimal ScoreMaximum, string Label, string Tag, string ResourceId, string ResourceLinkId, bool? GradesReleased);

    internal record ScoreRequest(string UserId, string ScoringUserId, decimal ScoreGiven, decimal ScoreMaximum, string Comment, ScoreSubmissionRequest? Submision, DateTime TimeStamp, ActivityProgress ActivityProgress, GradingProgress GradingProgress);

    internal record ScoreSubmissionRequest(DateTime? StartedAt, DateTime? SubmittedAt);

    internal static class Lti13ContentTypes
    {
        internal const string LineItemContainer = "application/vnd.ims.lis.v2.lineitemcontainer+json";
        internal const string LineItem = "application/vnd.ims.lis.v2.lineitem+json";
        internal const string ResultContainer = "application/vnd.ims.lis.v2.resultcontainer+json";
        internal const string Score = "application/vnd.ims.lis.v1.score+json";
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
