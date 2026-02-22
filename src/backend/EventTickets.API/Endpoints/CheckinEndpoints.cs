using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
            CancellationToken ct) =>
        {
            // TODO: Find ticket by QR code
            // TODO: Verify ticket is valid and not already checked in
            // TODO: Update ticket status
            // TODO: Log check-in time
            // TODO: Return attendee and event info

            return Results.Ok(new ApiResponse<object>(new { message = "Check-in will be implemented" }, "Check-in functionality pending Phase 04"));
        })
        .WithName("CheckInTicket")
        .WithSummary("Check in ticket by QR code")
        .WithOpenApi();

        // POST /api/checkin/validate - Validate ticket without checking in
        group.MapPost("/validate", async (
            [FromBody] CheckinRequest request,
            CancellationToken ct) =>
        {
            // TODO: Find ticket by QR code
            // TODO: Return ticket validity status
            // TODO: Show attendee info

            return Results.Ok(new ApiResponse<object>(new { message = "Validation will be implemented" }, "Ticket validation pending Phase 04"));
        })
        .WithName("ValidateTicket")
        .WithSummary("Validate ticket without checking in")
        .WithOpenApi();

        // POST /api/checkin/{ticketId}/undo - Undo check-in
        group.MapPost("/{ticketId:guid}/undo", async (
            Guid ticketId,
            CancellationToken ct) =>
        {
            // TODO: Verify admin/organizer permissions
            // TODO: Reset ticket check-in status
            // TODO: Log undo action

            return Results.Ok(new ApiResponse<object>(new { message = "Undo check-in will be implemented" }, "Check-in undo pending Phase 04"));
        })
        .WithName("UndoCheckIn")
        .WithSummary("Undo ticket check-in")
        .WithOpenApi();
    }
}
