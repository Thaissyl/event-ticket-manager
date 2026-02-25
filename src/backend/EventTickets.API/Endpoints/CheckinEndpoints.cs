using EventTickets.Core.DTOs;
using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;
using EventTickets.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EventTickets.API.Endpoints;

public static class CheckinEndpoints
{
    public static void MapCheckinEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/checkin")
            .WithTags("Check-in")
            .WithOpenApi();

        // POST /api/checkin - Check in ticket by QR code
        group.MapPost("", async (
            [FromBody] CheckinRequest request,
            [FromServices] ICheckinService checkinService,
            CancellationToken ct) =>
        {
            var result = await checkinService.CheckInAsync(request.QrCode, ct);
            return Results.Ok(new CheckinResponse(
                result.Success,
                result.Message,
                result.AttendeeName,
                result.EventName,
                result.CheckedInAt
            ));
        })
        .WithName("CheckInTicket")
        .WithSummary("Check in ticket by QR code")
        .WithOpenApi();

        // POST /api/checkin/validate - Validate ticket without checking in
        group.MapPost("/validate", async (
            [FromBody] CheckinRequest request,
            [FromServices] ICheckinService checkinService,
            CancellationToken ct) =>
        {
            var result = await checkinService.ValidateAsync(request.QrCode, ct);
            return Results.Ok(new TicketValidationResponse(
                result.Valid,
                result.Message,
                result.AttendeeName,
                result.TicketTierName,
                result.Status.ToString()
            ));
        })
        .WithName("ValidateTicket")
        .WithSummary("Validate ticket without checking in")
        .WithOpenApi();

        // POST /api/checkin/{ticketId}/undo - Undo check-in
        group.MapPost("/{ticketId:guid}/undo", async (
            Guid ticketId,
            [FromServices] ICheckinService checkinService,
            CancellationToken ct) =>
        {
            var success = await checkinService.UndoCheckInAsync(ticketId, ct);
            if (!success)
            {
                return Results.NotFound(new { message = "Ticket not found or not checked in" });
            }
            return Results.Ok(new { message = "Check-in undone successfully" });
        })
        .WithName("UndoCheckIn")
        .WithSummary("Undo ticket check-in")
        .WithOpenApi();

        // GET /api/events/{id}/checkin-stats - Get check-in statistics for an event
        app.MapGet("/api/events/{id:guid}/checkin-stats", async (
            Guid id,
            [FromServices] ICheckinService checkinService,
            CancellationToken ct) =>
        {
            var stats = await checkinService.GetStatsAsync(id, ct);
            return Results.Ok(new CheckinStatsResponse(
                stats.TotalSold,
                stats.Used,
                stats.Percentage,
                stats.ByTier.Select(t => new TierStatsResponse(
                    t.Name,
                    t.Sold,
                    t.Used
                )).ToArray()
            ));
        })
        .WithName("GetCheckinStats")
        .WithSummary("Get check-in statistics for an event")
        .WithOpenApi();
    }
}

public record TicketValidationResponse(
    bool Valid,
    string Message,
    string? AttendeeName,
    string? TicketTierName,
    string Status
);

public record CheckinStatsResponse(
    int TotalSold,
    int CheckedIn,
    double Percentage,
    TierStatsResponse[] ByTier
);

public record TierStatsResponse(
    string Name,
    int Sold,
    int CheckedIn
);
