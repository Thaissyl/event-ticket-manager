using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventTickets.API.Endpoints;

public static class EventEndpoints
{
    public static void MapEvents(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events")
            .WithTags("Events")
            .WithOpenApi();

        // GET /api/events - List all events (public, paginated)
        group.MapGet("", async (
            [FromServices] IEventRepository repo,
            [AsParameters] PagedRequest request,
            CancellationToken ct) =>
        {
            var allEvents = await repo.GetAllAsync(ct);
            var totalCount = allEvents.Count();
            var events = allEvents
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            var response = new EventListResponse(
                events.Select(e => new EventResponse(
                    e.Id,
                    e.Title,
                    e.Description,
                    e.VenueName,
                    e.VenueAddress,
                    e.VenueCity,
                    e.StartDateTime,
                    e.EndDateTime,
                    e.Status,
                    e.ImageUrl,
                    e.TotalCapacity,
                    e.CreatedAt
                )),
                request.Page,
                request.PageSize,
                totalCount,
                (int)Math.Ceiling(totalCount / (double)request.PageSize)
            );

            return Results.Ok(response);
        })
        .WithName("GetEvents")
        .WithSummary("Get all events");

        // GET /api/events/{id} - Get event details
        group.MapGet("/{id:guid}", async (
            Guid id,
            [FromServices] IEventRepository repo,
            CancellationToken ct) =>
        {
            var eventEntity = await repo.GetWithTiersAsync(id, ct);
            if (eventEntity is null)
                return Results.NotFound();

            var response = new EventResponse(
                eventEntity.Id,
                eventEntity.Title,
                eventEntity.Description,
                eventEntity.VenueName,
                eventEntity.VenueAddress,
                eventEntity.VenueCity,
                eventEntity.StartDateTime,
                eventEntity.EndDateTime,
                eventEntity.Status,
                eventEntity.ImageUrl,
                eventEntity.TotalCapacity,
                eventEntity.CreatedAt
            );

            return Results.Ok(response);
        })
        .WithName("GetEventById")
        .WithSummary("Get event by ID");

        // GET /api/events/my - Get organizer's events (requires auth)
        group.MapGet("/my", async (
            HttpContext httpContext,
            [FromServices] IEventRepository repo,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var events = await repo.GetByOrganizerAsync(userId, ct);
            var response = events.Select(e => new EventResponse(
                e.Id,
                e.Title,
                e.Description,
                e.VenueName,
                e.VenueAddress,
                e.VenueCity,
                e.StartDateTime,
                e.EndDateTime,
                e.Status,
                e.ImageUrl,
                e.TotalCapacity,
                e.CreatedAt
            ));

            return Results.Ok(response);
        })
        .WithName("GetMyEvents")
        .WithSummary("Get current user's events")
        .RequireAuthorization();

        // POST /api/events - Create event (organizer only)
        group.MapPost("", async (
            [FromBody] CreateEventRequest request,
            HttpContext httpContext,
            [FromServices] IEventService eventService,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            // Validate request
            if (request.StartDateTime <= DateTime.UtcNow)
                return Results.BadRequest(new ApiError("INVALID_DATE", "Start date must be in the future"));

            if (request.EndDateTime <= request.StartDateTime)
                return Results.BadRequest(new ApiError("INVALID_DATE", "End date must be after start date"));

            if (request.TotalCapacity <= 0)
                return Results.BadRequest(new ApiError("INVALID_CAPACITY", "Total capacity must be greater than 0"));

            var eventEntity = await eventService.CreateEventAsync(request, userId, ct);
            var response = new EventResponse(
                eventEntity.Id,
                eventEntity.Title,
                eventEntity.Description,
                eventEntity.VenueName,
                eventEntity.VenueAddress,
                eventEntity.VenueCity,
                eventEntity.StartDateTime,
                eventEntity.EndDateTime,
                eventEntity.Status,
                eventEntity.ImageUrl,
                eventEntity.TotalCapacity,
                eventEntity.CreatedAt
            );

            return Results.Created($"/api/events/{eventEntity.Id}", response);
        })
        .WithName("CreateEvent")
        .WithSummary("Create a new event")
        .RequireAuthorization();

        // PUT /api/events/{id} - Update event (owner only)
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateEventRequest request,
            HttpContext httpContext,
            [FromServices] IEventService eventService,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var eventEntity = await eventService.UpdateEventAsync(id, request, userId, ct);
            if (eventEntity is null)
                return Results.NotFound(new ApiError("NOT_FOUND", "Event not found or access denied"));

            var response = new EventResponse(
                eventEntity.Id,
                eventEntity.Title,
                eventEntity.Description,
                eventEntity.VenueName,
                eventEntity.VenueAddress,
                eventEntity.VenueCity,
                eventEntity.StartDateTime,
                eventEntity.EndDateTime,
                eventEntity.Status,
                eventEntity.ImageUrl,
                eventEntity.TotalCapacity,
                eventEntity.CreatedAt
            );

            return Results.Ok(response);
        })
        .WithName("UpdateEvent")
        .WithSummary("Update an event")
        .RequireAuthorization();

        // DELETE /api/events/{id} - Delete event (owner only, draft only)
        group.MapDelete("/{id:guid}", async (
            Guid id,
            HttpContext httpContext,
            [FromServices] IEventService eventService,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var deleted = await eventService.DeleteEventAsync(id, userId, ct);
            if (!deleted)
                return Results.NotFound(new ApiError("NOT_FOUND", "Event not found, access denied, or cannot be deleted"));

            return Results.NoContent();
        })
        .WithName("DeleteEvent")
        .WithSummary("Delete an event (draft only)")
        .RequireAuthorization();

        // POST /api/events/{id}/publish - Publish event (owner only)
        group.MapPost("/{id:guid}/publish", async (
            Guid id,
            HttpContext httpContext,
            [FromServices] IEventService eventService,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var published = await eventService.PublishEventAsync(id, userId, ct);
            if (!published)
                return Results.BadRequest(new ApiError("PUBLISH_FAILED", "Event not found, access denied, or cannot be published"));

            return Results.Ok(new { message = "Event published successfully" });
        })
        .WithName("PublishEvent")
        .WithSummary("Publish an event")
        .RequireAuthorization();

        // POST /api/events/{id}/cancel - Cancel event (owner only)
        group.MapPost("/{id:guid}/cancel", async (
            Guid id,
            HttpContext httpContext,
            [FromServices] IEventService eventService,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var cancelled = await eventService.CancelEventAsync(id, userId, ct);
            if (!cancelled)
                return Results.BadRequest(new ApiError("CANCEL_FAILED", "Event not found, access denied, or cannot be cancelled"));

            return Results.Ok(new { message = "Event cancelled successfully" });
        })
        .WithName("CancelEvent")
        .WithSummary("Cancel an event")
        .RequireAuthorization();

        // POST /api/events/{id}/complete - Complete event (owner only)
        group.MapPost("/{id:guid}/complete", async (
            Guid id,
            HttpContext httpContext,
            [FromServices] IEventService eventService,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var completed = await eventService.CompleteEventAsync(id, userId, ct);
            if (!completed)
                return Results.BadRequest(new ApiError("COMPLETE_FAILED", "Event not found, access denied, or cannot be completed"));

            return Results.Ok(new { message = "Event completed successfully" });
        })
        .WithName("CompleteEvent")
        .WithSummary("Mark an event as completed")
        .RequireAuthorization();
    }
}
