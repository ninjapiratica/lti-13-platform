﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using NP.Lti13Platform.AssignmentGradeServices.Configs;
using NP.Lti13Platform.AssignmentGradeServices.Populators;
using NP.Lti13Platform.AssignmentGradeServices.Services;
using NP.Lti13Platform.Core;
using NP.Lti13Platform.Core.Constants;
using NP.Lti13Platform.Core.Models;
using NP.Lti13Platform.Core.Services;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace NP.Lti13Platform.AssignmentGradeServices;

public static class Startup
{
    public static Lti13PlatformBuilder AddLti13PlatformAssignmentGradeServices(this Lti13PlatformBuilder builder)
    {
        builder.ExtendLti13Message<IServiceEndpoints, ServiceEndpointsPopulator>();

        builder.Services.AddOptions<ServicesConfig>().BindConfiguration("Lti13Platform:AssignmentGradeServices");
        builder.Services.TryAddSingleton<ILti13AssignmentGradeConfigService, DefaultAssignmentGradeConfigService>();

        return builder;
    }

    public static Lti13PlatformBuilder WithLti13AssignmentGradeDataService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13AssignmentGradeDataService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13AssignmentGradeDataService), typeof(T), serviceLifetime));
        return builder;
    }

    public static Lti13PlatformBuilder WithLti13AssignmentGradeConfigService<T>(this Lti13PlatformBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : ILti13AssignmentGradeConfigService
    {
        builder.Services.Add(new ServiceDescriptor(typeof(ILti13AssignmentGradeConfigService), typeof(T), serviceLifetime));
        return builder;
    }

    public static IEndpointRouteBuilder UseLti13PlatformAssignmentGradeServices(this IEndpointRouteBuilder endpointRouteBuilder, Func<ServiceEndpointsConfig, ServiceEndpointsConfig>? configure = null, string openAPIGroupName = "")
    {
        const string OpenAPI_Tag = "LTI 1.3 Assignment and Grade Services";

        ServiceEndpointsConfig config = new();
        config = configure?.Invoke(config) ?? config;

        endpointRouteBuilder.MapGet(config.LineItemsUrl,
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
            })
            .WithGroupName(openAPIGroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Gets the line items within a context.")
            .WithDescription("Gets the line items within a context. Can be filtered by resource id, resource link id, or tag. It is a paginated request so page size and index may be provided. Pagination information (next, previous, etc) will be returned as headers.");

        endpointRouteBuilder.MapPost(config.LineItemsUrl,
            async (IHttpContextAccessor httpContextAccessor, ILti13CoreDataService coreDataService, ILti13AssignmentGradeDataService assignmentGradeDataService, LinkGenerator linkGenerator, string deploymentId, string contextId, LineItemRequest request, CancellationToken cancellationToken) =>
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

                if (!MediaTypeHeaderValue.TryParse(httpContext.Request.ContentType, out var headerValue) || headerValue.MediaType != ContentTypes.LineItem)
                {
                    return Results.BadRequest(new LtiBadRequest { Error = "Invalid Content-Type", Error_Description = $"Content-Type must be '{ContentTypes.LineItem}'", Error_Uri = "https://www.imsglobal.org/spec/lti-ags/v2p0/#creating-a-new-line-item" });
                }

                if (string.IsNullOrWhiteSpace(request.Label))
                {
                    return Results.BadRequest(new LtiBadRequest { Error = "Invalid Label", Error_Description = "Label is reuired", Error_Uri = "https://www.imsglobal.org/spec/lti-ags/v2p0/#label" });
                }

                if (request.ScoreMaximum <= 0)
                {
                    return Results.BadRequest(new LtiBadRequest { Error = "Invalid ScoreMaximum", Error_Description = "ScoreMaximum must be greater than 0", Error_Uri = "https://www.imsglobal.org/spec/lti-ags/v2p0/#scoremaximum" });
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
            .DisableAntiforgery()
            .WithGroupName(openAPIGroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Creates a line item within a context.")
            .WithDescription("Creates a line item within a context."); 

        endpointRouteBuilder.MapGet(config.LineItemUrl,
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
            })
            .WithGroupName(openAPIGroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Gets a line item within a context.")
            .WithDescription("Gets a line item within a context.");

        endpointRouteBuilder.MapPut(config.LineItemUrl,
            async (IHttpContextAccessor httpContextAccessor, ILti13CoreDataService coreDataService, ILti13AssignmentGradeDataService assignmentGradeDataService, LinkGenerator linkGenerator, string deploymentId, string contextId, string lineItemId, LineItemRequest request, CancellationToken cancellationToken) =>
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

                if (!MediaTypeHeaderValue.TryParse(httpContext.Request.ContentType, out var headerValue) || headerValue.MediaType != ContentTypes.LineItem)
                {
                    return Results.BadRequest(new LtiBadRequest { Error = "Invalid Content-Type", Error_Description = $"Content-Type must be '{ContentTypes.LineItem}'", Error_Uri = "https://www.imsglobal.org/spec/lti-ags/v2p0/#creating-a-new-line-item" });
                }

                if (string.IsNullOrWhiteSpace(request.Label))
                {
                    return Results.BadRequest(new LtiBadRequest { Error = "Invalid Label", Error_Description = "Label is reuired", Error_Uri = "https://www.imsglobal.org/spec/lti-ags/v2p0/#label" });
                }

                if (request.ScoreMaximum <= 0)
                {
                    return Results.BadRequest(new LtiBadRequest { Error = "Invalid ScoreMaximum", Error_Description = "ScoreMaximum must be greater than 0", Error_Uri = "https://www.imsglobal.org/spec/lti-ags/v2p0/#scoremaximum" });
                }

                if (!string.IsNullOrWhiteSpace(request.ResourceLinkId) && request.ResourceLinkId != lineItem.ResourceLinkId)
                {
                    return Results.BadRequest(new LtiBadRequest { Error = "Invalid ResourceLinkId", Error_Description = "ResourceLinkId may not change after creation", Error_Uri = "https://www.imsglobal.org/spec/lti-ags/v2p0/#updating-a-line-item" });
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
            .DisableAntiforgery()
            .WithGroupName(openAPIGroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Updates a line item within a context.")
            .WithDescription("Updates a line item within a context.");

        endpointRouteBuilder.MapDelete(config.LineItemUrl,
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
            .DisableAntiforgery()
            .WithGroupName(openAPIGroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Deletes a line item within a context.")
            .WithDescription("Deletes a line item within a context.");

        endpointRouteBuilder.MapGet($"{config.LineItemUrl}/results",
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
            })
            .WithGroupName(openAPIGroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Gets the results within a context and line item.")
            .WithDescription("Gets the results within a context and line item. Can be filtered by user id. It is a paginated request so page size and index may be provided. Pagination information (next, previous, etc) will be returned as headers.");

        endpointRouteBuilder.MapPost($"{config.LineItemUrl}/scores",
            async (IHttpContextAccessor httpContextAccessor, ILti13CoreDataService coreDataService, ILti13AssignmentGradeDataService assignmentGradeDataService, string deploymentId, string contextId, string lineItemId, ScoreRequest request, CancellationToken cancellationToken) =>
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

                if (DateTime.UtcNow < lineItem.StartDateTime)
                {
                    return Results.Json(new
                    {
                        Error = "startDateTime",
                        Error_Description = "lineItem startDateTime is in the future",
                        Error_Uri = "https://www.imsglobal.org/spec/lti-ags/v2p0/#startdatetime"
                    }, statusCode: (int)HttpStatusCode.Forbidden);
                }

                if (DateTime.UtcNow > lineItem.EndDateTime)
                {
                    return Results.Json(new
                    {
                        Error = "endDateTime",
                        Error_Description = "lineItem endDateTime is in the past",
                        Error_Uri = "https://www.imsglobal.org/spec/lti-ags/v2p0/#enddatetime"
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
                        Error = "timestamp",
                        Error_Description = "timestamp must be after the current timestamp",
                        Error_Uri = "https://www.imsglobal.org/spec/lti-ags/v2p0/#timestamp"
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
            .DisableAntiforgery()
            .WithGroupName(openAPIGroupName)
            .WithTags(OpenAPI_Tag)
            .WithSummary("Creates or updates a score within a context.")
            .WithDescription("Creates or updates a score within a context.");

        return endpointRouteBuilder;
    }
}

internal record LineItemRequest(decimal ScoreMaximum, string Label, string? ResourceLinkId, string? ResourceId, string? Tag, bool? GradesReleased, DateTimeOffset? StartDateTime, DateTimeOffset? EndDateTime);

internal record ScoreRequest(string UserId, string ScoringUserId, decimal? ScoreGiven, decimal? ScoreMaximum, string Comment, ScoreSubmissionRequest? Submission, DateTimeOffset TimeStamp, string ActivityProgress, string GradingProgress);

internal record ScoreSubmissionRequest(DateTimeOffset? StartedAt, DateTimeOffset? SubmittedAt);
