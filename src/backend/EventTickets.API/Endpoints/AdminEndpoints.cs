using EventTickets.Core.DTOs;
using EventTickets.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventTickets.API.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .WithOpenApi();

        // GET /api/admin/users - List all users
        group.MapGet("/users", async (CancellationToken ct) =>
        {
            // TODO: Add admin authorization
            // TODO: Return paginated user list with order counts

            return Results.Ok(new ApiResponse<object>(new { message = "Admin endpoints will be implemented" }, "Admin features pending Phase 04"));
        })
        .WithName("GetAllUsers")
        .WithSummary("List all users (admin only)")
        .WithOpenApi();

        // GET /api/admin/events - List all events (including drafts)
        group.MapGet("/events", async (
            [FromServices] IEventRepository eventRepo,
            CancellationToken ct) =>
        {
            // TODO: Add admin authorization
            // TODO: Return all events regardless of status
            // TODO: Include organizer info

            return Results.Ok(new ApiResponse<object>(new { message = "Admin endpoints will be implemented" }, "Admin features pending Phase 04"));
        })
        .WithName("GetAllEvents")
        .WithSummary("List all events (admin only)")
        .WithOpenApi();

        // GET /api/admin/stats - Get platform statistics
        group.MapGet("/stats", async (
            [FromServices] IEventRepository eventRepo,
            CancellationToken ct) =>
        {
            // TODO: Add admin authorization
            // TODO: Calculate platform-wide statistics

            var stats = new PlatformStatsResponse(
                TotalUsers: 0,
                TotalEvents: await eventRepo.CountAsync(ct),
                PublishedEvents: 0,
                TotalOrders: 0,
                TotalRevenue: 0,
                ActiveEvents: 0
            );

            return Results.Ok(new ApiResponse<PlatformStatsResponse>(stats));
        })
        .WithName("GetPlatformStats")
        .WithSummary("Get platform statistics (admin only)")
        .WithOpenApi();

        // PUT /api/admin/events/{id}/status - Update event status
        group.MapPut("/events/{id:guid}/status", async (
            Guid id,
            [FromBody] UpdateEventStatusRequest request,
            [FromServices] IEventRepository eventRepo,
            CancellationToken ct) =>
        {
            // TODO: Add admin authorization
            // TODO: Update event status
            // TODO: Notify organizer

            return Results.Ok(new ApiResponse<object>(new { message = "Admin endpoints will be implemented" }, "Admin features pending Phase 04"));
        })
        .WithName("UpdateEventStatus")
        .WithSummary("Update event status (admin only)")
        .WithOpenApi();
    }
}

public record UpdateEventStatusRequest(string Status);
