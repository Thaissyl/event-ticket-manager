using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    }
}
