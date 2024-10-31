using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using NP.Lti13Platform.AssignmentGradeServices.Configs;
using NP.Lti13Platform.AssignmentGradeServices.Populators;
using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace NP.Lti13Platform.AssignmentGradeServices
{
    public static class Startup
    {
        public static Lti13PlatformBuilder AddLti13PlatformAssignmentGradeServices(this Lti13PlatformBuilder builder)
        {
            builder.ExtendLti13Message<IServiceEndpoints, ServiceEndpointsPopulator>();

            return builder;
        }

        public static Lti13PlatformBuilder WithDefaultAssignmentGradeService(this Lti13PlatformBuilder builder, Action<ServicesConfig>? configure = null)
        {
            configure ??= (x) => { };

            builder.Services.Configure(configure);
            builder.Services.AddTransient<ILti13AssignmentGradeConfigService, DefaultAssignmentGradeConfigService>();
            return builder;
        }

        public static IEndpointRouteBuilder UseLti13PlatformAssignmentGradeServices(this IEndpointRouteBuilder app, Func<ServiceEndpointsConfig, ServiceEndpointsConfig>? configure = null)
        {
            ServiceEndpointsConfig config = new();
            config = configure?.Invoke(config) ?? config;

            app.MapGet(config.LineItemsUrl,
                async (IHttpContextAccessor httpContextAccessor, ILti13CoreDataService coreDataService, LinkGenerator linkGenerator, string deploymentId, string contextId, string? resource_id, string? resource_link_id, string? tag, int? limit, int pageIndex = 0, CancellationToken cancellationToken = default) =>
                {
                    var httpContext = httpContextAccessor.HttpContext!;
                    var clientId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

                    var tool = await coreDataService.GetToolAsync(clientId, cancellationToken);
                    if (tool == null)
                    {
                        return Results.NotFound();
                    }

                    var deployment = await coreDataService.GetDeploymentAsync(deploymentId, cancellationToken);
                    if (deployment?.ToolId != tool.Id)
                    {
                        return Results.NotFound();
                    }

                    var context = await coreDataService.GetContextAsync(contextId, cancellationToken);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItemsResponse = await coreDataService.GetLineItemsAsync(deploymentId, contextId, pageIndex, limit ?? int.MaxValue, resource_id, resource_link_id, tag, cancellationToken);

                    if (lineItemsResponse.TotalItems > 0 && limit.HasValue)
                    {
                        var links = new Collection<string>();
                        if (pageIndex > 0)
                        {
                            links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEMS, new { deploymentId, contextId, resource_id, resource_link_id, tag, limit, pageIndex = pageIndex - 1 })}>; rel=\"prev\"");
                        }

                        if (lineItemsResponse.TotalItems > limit * (pageIndex + 1))
                        {
                            links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEMS, new { deploymentId, contextId, resource_id, resource_link_id, tag, limit, pageIndex = pageIndex + 1 })}>; rel=\"next\"");
                        }

                        links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEMS, new { deploymentId, contextId, resource_id, resource_link_id, tag, limit, pageIndex = 0 })}>; rel=\"first\"");

                        links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEMS, new { deploymentId, contextId, resource_id, resource_link_id, tag, limit, pageIndex = Math.Ceiling(lineItemsResponse.TotalItems * 1.0 / limit.GetValueOrDefault()) - 1 })}>; rel=\"last\"");

                        httpContext.Response.Headers.Link = new StringValues([.. links]);
                    }

                    return Results.Json(lineItemsResponse.Items.Select(i => new
                    {
                        Id = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { deploymentId, contextId, lineItemId = i.Id }),
                        i.StartDateTime,
                        i.EndDateTime,
                        i.ScoreMaximum,
                        i.Label,
                        i.Tag,
                        i.ResourceId,
                        i.ResourceLinkId
                    }), contentType: ContentTypes.LineItemContainer);
                })
                .WithName(RouteNames.GET_LINE_ITEMS)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(ServiceScopes.LineItem, ServiceScopes.LineItemReadOnly);
                });

            app.MapPost(config.LineItemsUrl,
                async (IHttpContextAccessor httpContextAccessor, ILti13CoreDataService coreDataService, ILti13AssignmentGradeDataService assignmentGradeDataService, LinkGenerator linkGenerator, string deploymentId, string contextId, LineItemRequest request, CancellationToken cancellationToken) =>
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
                    var clientId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

                    var tool = await coreDataService.GetToolAsync(clientId, cancellationToken);
                    if (tool == null)
                    {
                        return Results.NotFound();
                    }

                    var deployment = await coreDataService.GetDeploymentAsync(deploymentId, cancellationToken);
                    if (deployment?.ToolId != tool.Id)
                    {
                        return Results.NotFound();
                    }

                    var context = await coreDataService.GetContextAsync(contextId, cancellationToken);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    if (!MediaTypeHeaderValue.TryParse(httpContext.Request.ContentType, out var headerValue) || headerValue.MediaType != ContentTypes.LineItem)
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
                        var resourceLink = await coreDataService.GetResourceLinkAsync(request.ResourceLinkId, cancellationToken);
                        if (resourceLink?.DeploymentId != deploymentId || resourceLink.ContextId != contextId)
                        {
                            return Results.NotFound();
                        }
                    }

                    var lineItemId = await assignmentGradeDataService.SaveLineItemAsync(new LineItem
                    {
                        Id = string.Empty,
                        DeploymentId = deploymentId,
                        ContextId = contextId,
                        Label = request.Label,
                        ResourceId = request.ResourceId,
                        ResourceLinkId = request.ResourceLinkId,
                        ScoreMaximum = request.ScoreMaximum,
                        Tag = request.Tag,
                        GradesReleased = request.GradesReleased,
                        StartDateTime = request.StartDateTime?.UtcDateTime,
                        EndDateTime = request.EndDateTime?.UtcDateTime,
                    }, cancellationToken);

                    var url = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { deploymentId, contextId, lineItemId });
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
                    policy.RequireRole(ServiceScopes.LineItem);
                })
                .DisableAntiforgery();

            app.MapGet(config.LineItemUrl,
                async (IHttpContextAccessor httpContextAccessor, ILti13CoreDataService coreDataService, ILti13AssignmentGradeDataService assignmentGradeDataService, LinkGenerator linkGenerator, string deploymentId, string contextId, string lineItemId, CancellationToken cancellationToken) =>
                {
                    var httpContext = httpContextAccessor.HttpContext!;
                    var clientId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

                    var tool = await coreDataService.GetToolAsync(clientId, cancellationToken);
                    if (tool == null)
                    {
                        return Results.NotFound();
                    }

                    var deployment = await coreDataService.GetDeploymentAsync(deploymentId, cancellationToken);
                    if (deployment?.ToolId != tool.Id)
                    {
                        return Results.NotFound();
                    }

                    var context = await coreDataService.GetContextAsync(contextId, cancellationToken);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItem = await assignmentGradeDataService.GetLineItemAsync(lineItemId, cancellationToken);
                    if (lineItem?.DeploymentId != deploymentId || lineItem.ContextId != contextId)
                    {
                        return Results.NotFound();
                    }

                    return Results.Json(new
                    {
                        Id = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { deploymentId, contextId, lineItemId }),
                        lineItem.Label,
                        lineItem.ResourceId,
                        lineItem.ResourceLinkId,
                        lineItem.ScoreMaximum,
                        lineItem.Tag,
                        lineItem.StartDateTime,
                        lineItem.EndDateTime,
                    }, contentType: ContentTypes.LineItem);
                })
                .WithName(RouteNames.GET_LINE_ITEM)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(ServiceScopes.LineItem, ServiceScopes.LineItemReadOnly);
                });

            app.MapPut(config.LineItemUrl,
                async (IHttpContextAccessor httpContextAccessor, ILti13CoreDataService coreDataService, ILti13AssignmentGradeDataService assignmentGradeDataService, LinkGenerator linkGenerator, string deploymentId, string contextId, string lineItemId, LineItemRequest request, CancellationToken cancellationToken) =>
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
                    var clientId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

                    var tool = await coreDataService.GetToolAsync(clientId, cancellationToken);
                    if (tool == null)
                    {
                        return Results.NotFound();
                    }

                    var deployment = await coreDataService.GetDeploymentAsync(deploymentId, cancellationToken);
                    if (deployment?.ToolId != tool.Id)
                    {
                        return Results.NotFound();
                    }

                    var context = await coreDataService.GetContextAsync(contextId, cancellationToken);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItem = await assignmentGradeDataService.GetLineItemAsync(lineItemId, cancellationToken);
                    if (lineItem?.DeploymentId != deploymentId || lineItem.ContextId != contextId)
                    {
                        return Results.NotFound();
                    }

                    if (!MediaTypeHeaderValue.TryParse(httpContext.Request.ContentType, out var headerValue) || headerValue.MediaType != ContentTypes.LineItem)
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
                    lineItem.ResourceId = request.ResourceId;
                    lineItem.ResourceLinkId = request.ResourceLinkId;
                    lineItem.ScoreMaximum = request.ScoreMaximum;
                    lineItem.Tag = request.Tag;
                    lineItem.GradesReleased = request.GradesReleased;
                    lineItem.StartDateTime = request.StartDateTime?.UtcDateTime;
                    lineItem.EndDateTime = request.EndDateTime?.UtcDateTime;

                    await assignmentGradeDataService.SaveLineItemAsync(lineItem, cancellationToken);

                    return Results.Json(new
                    {
                        Id = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { deploymentId, contextId, lineItemId }),
                        lineItem.Label,
                        lineItem.ResourceId,
                        lineItem.ResourceLinkId,
                        lineItem.ScoreMaximum,
                        lineItem.Tag,
                        lineItem.GradesReleased,
                        lineItem.StartDateTime,
                        lineItem.EndDateTime,
                    }, contentType: ContentTypes.LineItem);
                })
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(ServiceScopes.LineItem);
                })
                .DisableAntiforgery();

            app.MapDelete(config.LineItemUrl,
                async (IHttpContextAccessor httpContextAccessor, ILti13CoreDataService coreDataService, ILti13AssignmentGradeDataService assignmentGradeDataService, string deploymentId, string contextId, string lineItemId, CancellationToken cancellationToken) =>
                {
                    var httpContext = httpContextAccessor.HttpContext!;
                    var clientId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

                    var tool = await coreDataService.GetToolAsync(clientId, cancellationToken);
                    if (tool == null)
                    {
                        return Results.NotFound();
                    }

                    var deployment = await coreDataService.GetDeploymentAsync(deploymentId, cancellationToken);
                    if (deployment?.ToolId != tool.Id)
                    {
                        return Results.NotFound();
                    }

                    var context = await coreDataService.GetContextAsync(contextId, cancellationToken);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItem = await assignmentGradeDataService.GetLineItemAsync(lineItemId, cancellationToken);
                    if (lineItem?.DeploymentId != deploymentId || lineItem.ContextId != contextId)
                    {
                        return Results.NotFound();
                    }

                    await assignmentGradeDataService.DeleteLineItemAsync(lineItemId, cancellationToken);

                    return Results.NoContent();
                })
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(ServiceScopes.LineItem);
                })
                .DisableAntiforgery();

            app.MapGet($"{config.LineItemUrl}/results",
                async (IHttpContextAccessor httpContextAccessor, ILti13CoreDataService coreDataService, ILti13AssignmentGradeDataService assignmentGradeDataService, LinkGenerator linkGenerator, string deploymentId, string contextId, string lineItemId, string? user_id, int? limit, int pageIndex = 0, CancellationToken cancellationToken = default) =>
                {
                    var httpContext = httpContextAccessor.HttpContext!;
                    var clientId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

                    var tool = await coreDataService.GetToolAsync(clientId, cancellationToken);
                    if (tool == null)
                    {
                        return Results.NotFound();
                    }

                    var deployment = await coreDataService.GetDeploymentAsync(deploymentId, cancellationToken);
                    if (deployment?.ToolId != tool.Id)
                    {
                        return Results.NotFound();
                    }

                    var context = await coreDataService.GetContextAsync(contextId, cancellationToken);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItem = await assignmentGradeDataService.GetLineItemAsync(lineItemId, cancellationToken);
                    if (lineItem?.DeploymentId != deploymentId || lineItem.ContextId != contextId)
                    {
                        return Results.NotFound();
                    }

                    var gradesResponse = await assignmentGradeDataService.GetGradesAsync(lineItemId, pageIndex, limit ?? int.MaxValue, user_id, cancellationToken);

                    if (gradesResponse.TotalItems > 0 && limit.HasValue)
                    {
                        var links = new Collection<string>();
                        if (pageIndex > 0)
                        {
                            links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM_RESULTS, new { deploymentId, contextId, lineItemId, limit, pageIndex = pageIndex - 1 })}>; rel=\"prev\"");
                        }

                        if (gradesResponse.TotalItems > limit * (pageIndex + 1))
                        {
                            links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM_RESULTS, new { deploymentId, contextId, lineItemId, limit, pageIndex = pageIndex + 1 })}>; rel=\"next\"");
                        }

                        links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM_RESULTS, new { deploymentId, contextId, lineItemId, limit, pageIndex = 0 })}>; rel=\"first\"");

                        links.Add($"<{linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM_RESULTS, new { deploymentId, contextId, lineItemId, limit, pageIndex = Math.Ceiling(gradesResponse.TotalItems * 1.0 / limit.GetValueOrDefault()) - 1 })}>; rel=\"last\"");

                        httpContext.Response.Headers.Link = new StringValues([.. links]);
                    }

                    return Results.Json(gradesResponse.Items.Select(i => new
                    {
                        Id = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM_RESULTS, new { deploymentId, contextId, i.LineItemId, user_id = i.UserId }),
                        ScoreOf = linkGenerator.GetUriByName(httpContext, RouteNames.GET_LINE_ITEM, new { deploymentId, contextId, i.LineItemId }),
                        i.UserId,
                        i.ResultScore,
                        ResultMaximum = i.ResultMaximum ?? 1, // https://www.imsglobal.org/spec/lti-ags/v2p0/#resultmaximum
                        i.ScoringUserId,
                        i.Comment
                    }), contentType: ContentTypes.ResultContainer);
                })
                .WithName(RouteNames.GET_LINE_ITEM_RESULTS)
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(ServiceScopes.ResultReadOnly);
                });

            app.MapPost($"{config.LineItemUrl}/scores",
                async (IHttpContextAccessor httpContextAccessor, ILti13CoreDataService coreDataService, ILti13AssignmentGradeDataService assignmentGradeDataService, string deploymentId, string contextId, string lineItemId, ScoreRequest request, CancellationToken cancellationToken) =>
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

                    var httpContext = httpContextAccessor.HttpContext!;
                    var clientId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

                    var tool = await coreDataService.GetToolAsync(clientId, cancellationToken);
                    if (tool == null)
                    {
                        return Results.NotFound();
                    }

                    var deployment = await coreDataService.GetDeploymentAsync(deploymentId, cancellationToken);
                    if (deployment?.ToolId != tool.Id)
                    {
                        return Results.NotFound();
                    }

                    var context = await coreDataService.GetContextAsync(contextId, cancellationToken);
                    if (context == null)
                    {
                        return Results.NotFound();
                    }

                    var lineItem = await assignmentGradeDataService.GetLineItemAsync(lineItemId, cancellationToken);
                    if (lineItem?.DeploymentId != deploymentId || lineItem.ContextId != contextId)
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
                    var grade = await coreDataService.GetGradeAsync(lineItemId, request.UserId, cancellationToken);
                    if (grade == null)
                    {
                        isNew = true;
                        grade = new Grade
                        {
                            LineItemId = lineItemId,
                            UserId = request.UserId
                        };
                    }
                    else if (grade.Timestamp >= request.TimeStamp)
                    {
                        return Results.Conflict(new
                        {
                            Error = OUT_OF_DATE,
                            Error_Description = OUT_OF_DATE_DESCRIPTION,
                            Error_Uri = OUT_OF_DATE_URI
                        });
                    }

                    grade.ResultScore = request.ScoreGiven;
                    grade.ResultMaximum = request.ScoreMaximum;
                    grade.Comment = request.Comment;
                    grade.ScoringUserId = request.ScoringUserId;
                    grade.Timestamp = request.TimeStamp.UtcDateTime;
                    grade.ActivityProgress = Enum.Parse<ActivityProgress>(request.ActivityProgress);
                    grade.GradingProgress = Enum.Parse<GradingProgress>(request.GradingProgress);

                    if (request.Submission?.StartedAt != null)
                    {
                        grade.StartedAt = request.Submission.StartedAt?.UtcDateTime;
                    }
                    else if (grade.ActivityProgress == ActivityProgress.Initialized)
                    {
                        grade.StartedAt = null;
                    }
                    else if (grade.StartedAt == null && (grade.ActivityProgress == ActivityProgress.Started || grade.ActivityProgress == ActivityProgress.InProgress))
                    {
                        grade.StartedAt = DateTime.UtcNow;
                    }

                    if (request.Submission?.SubmittedAt != null)
                    {
                        grade.SubmittedAt = request.Submission.SubmittedAt?.UtcDateTime;
                    }
                    else if (grade.ActivityProgress == ActivityProgress.Initialized || grade.ActivityProgress == ActivityProgress.Started || grade.ActivityProgress == ActivityProgress.InProgress)
                    {
                        grade.SubmittedAt = null;
                    }
                    else if (grade.SubmittedAt == null && (grade.ActivityProgress == ActivityProgress.Submitted || grade.ActivityProgress == ActivityProgress.Completed))
                    {
                        grade.SubmittedAt = DateTime.UtcNow;
                    }

                    await assignmentGradeDataService.SaveGradeAsync(grade, cancellationToken);

                    return isNew ? Results.Created() : Results.NoContent();
                })
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(LtiServicesAuthHandler.SchemeName);
                    policy.RequireRole(ServiceScopes.Score);
                })
                .DisableAntiforgery();

            return app;
        }
    }

    internal record LineItemRequest(decimal ScoreMaximum, string Label, string? ResourceLinkId, string? ResourceId, string? Tag, bool? GradesReleased, DateTimeOffset? StartDateTime, DateTimeOffset? EndDateTime);

    internal record ScoreRequest(string UserId, string ScoringUserId, decimal? ScoreGiven, decimal? ScoreMaximum, string Comment, ScoreSubmissionRequest? Submission, DateTimeOffset TimeStamp, string ActivityProgress, string GradingProgress);

    internal record ScoreSubmissionRequest(DateTimeOffset? StartedAt, DateTimeOffset? SubmittedAt);
}
