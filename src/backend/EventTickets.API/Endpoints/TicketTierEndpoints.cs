using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventTickets.API.Endpoints;

public static class TicketTierEndpoints
{
    public static void MapTicketTierEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events/{eventId:guid}/tiers")
            .WithTags("Ticket Tiers")
            .WithOpenApi();

        // GET /api/events/{eventId}/tiers - List tiers for an event
        group.MapGet("", async (
            Guid eventId,
            [FromServices] ITicketTierRepository tierRepo,
            CancellationToken ct) =>
        {
            var tiers = await tierRepo.GetByEventAsync(eventId, ct);

            var response = tiers.Select(t => new TicketTierResponse(
                t.Id,
                t.EventId,
                t.Name,
                t.Description,
                t.Price,
                t.QuantityTotal,
                t.QuantitySold,
                t.QuantityTotal - t.QuantitySold - t.QuantityReserved,
                t.SaleStartDateTime,
                t.SaleEndDateTime
            ));

            return Results.Ok(response);
        })
        .WithName("GetTicketTiers")
        .WithSummary("Get ticket tiers for an event");

        // GET /api/events/{eventId}/tiers/{tierId} - Get specific tier
        group.MapGet("/{tierId:guid}", async (
            Guid eventId,
            Guid tierId,
            [FromServices] ITicketTierRepository tierRepo,
            CancellationToken ct) =>
        {
            var tier = await tierRepo.GetByIdAsync(tierId, ct);
            if (tier is null || tier.EventId != eventId)
                return Results.NotFound(new ApiError("NOT_FOUND", "Ticket tier not found"));

            var response = new TicketTierResponse(
                tier.Id,
                tier.EventId,
                tier.Name,
                tier.Description,
                tier.Price,
                tier.QuantityTotal,
                tier.QuantitySold,
                tier.QuantityTotal - tier.QuantitySold - tier.QuantityReserved,
                tier.SaleStartDateTime,
                tier.SaleEndDateTime
            );

            return Results.Ok(response);
        })
        .WithName("GetTicketTierById")
        .WithSummary("Get ticket tier by ID");

        // POST /api/events/{eventId}/tiers - Create tier (organizer only)
        group.MapPost("", async (
            Guid eventId,
            [FromBody] CreateTicketTierRequest request,
            [FromServices] IEventRepository eventRepo,
            [FromServices] ITicketTierRepository tierRepo,
            CancellationToken ct) =>
        {
            // TODO: Add authorization - verify user is event organizer
            var eventEntity = await eventRepo.GetByIdAsync(eventId, ct);
            if (eventEntity is null)
                return Results.NotFound(new ApiError("NOT_FOUND", "Event not found"));

            var tier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                QuantityTotal = request.QuantityTotal,
                QuantitySold = 0,
                QuantityReserved = 0,
                SaleStartDateTime = request.SaleStartDateTime,
                SaleEndDateTime = request.SaleEndDateTime
            };

            await tierRepo.AddAsync(tier, ct);
            await tierRepo.SaveChangesAsync(ct);

            var response = new TicketTierResponse(
                tier.Id,
                tier.EventId,
                tier.Name,
                tier.Description,
                tier.Price,
                tier.QuantityTotal,
                tier.QuantitySold,
                tier.QuantityTotal - tier.QuantitySold - tier.QuantityReserved,
                tier.SaleStartDateTime,
                tier.SaleEndDateTime
            );

            return Results.Created($"/api/events/{eventId}/tiers/{tier.Id}", response);
        })
        .WithName("CreateTicketTier")
        .WithSummary("Create a new ticket tier");

        // PUT /api/events/{eventId}/tiers/{tierId} - Update tier (organizer only)
        group.MapPut("/{tierId:guid}", async (
            Guid eventId,
            Guid tierId,
            [FromBody] UpdateTicketTierRequest request,
            [FromServices] ITicketTierRepository tierRepo,
            CancellationToken ct) =>
        {
            // TODO: Add authorization - verify user is event organizer
            var tier = await tierRepo.GetByIdAsync(tierId, ct);
            if (tier is null || tier.EventId != eventId)
                return Results.NotFound(new ApiError("NOT_FOUND", "Ticket tier not found"));

            tier.Name = request.Name;
            tier.Description = request.Description;
            tier.Price = request.Price;
            tier.QuantityTotal = request.QuantityTotal;
            tier.SaleStartDateTime = request.SaleStartDateTime;
            tier.SaleEndDateTime = request.SaleEndDateTime;

            await tierRepo.UpdateAsync(tier, ct);
            await tierRepo.SaveChangesAsync(ct);

            var response = new TicketTierResponse(
                tier.Id,
                tier.EventId,
                tier.Name,
                tier.Description,
                tier.Price,
                tier.QuantityTotal,
                tier.QuantitySold,
                tier.QuantityTotal - tier.QuantitySold - tier.QuantityReserved,
                tier.SaleStartDateTime,
                tier.SaleEndDateTime
            );

            return Results.Ok(response);
        })
        .WithName("UpdateTicketTier")
        .WithSummary("Update ticket tier");

        // DELETE /api/events/{eventId}/tiers/{tierId} - Delete tier (organizer only)
        group.MapDelete("/{tierId:guid}", async (
            Guid eventId,
            Guid tierId,
            [FromServices] ITicketTierRepository tierRepo,
            CancellationToken ct) =>
        {
            // TODO: Add authorization - verify user is event organizer
            var tier = await tierRepo.GetWithTicketsAsync(tierId, ct);
            if (tier is null || tier.EventId != eventId)
                return Results.NotFound(new ApiError("NOT_FOUND", "Ticket tier not found"));

            // TODO: Check if tier has sales - prevent deletion if tickets sold
            await tierRepo.DeleteAsync(tier, ct);
            await tierRepo.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteTicketTier")
        .WithSummary("Delete ticket tier");
    }
}
