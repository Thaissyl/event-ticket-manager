using EventTickets.Core.DTOs;
using EventTickets.Core.Enums;
using EventTickets.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventTickets.API.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .WithOpenApi();
        // TODO: Add .RequireAuthorization("Admin") when authorization is implemented

        // GET /api/admin/stats - Platform statistics
        group.MapGet("/stats", async (
            [FromServices] IAdminService adminService,
            CancellationToken ct = default) =>
        {
            var stats = await adminService.GetPlatformStatsAsync(ct);
            return Results.Ok(new ApiResponse<PlatformStatsResponse>(stats));
        })
        .WithName("GetPlatformStats")
        .WithSummary("Get platform statistics (admin only)")
        .WithOpenApi();

        // GET /api/admin/users - List users (paginated, searchable)
        group.MapGet("/users", async (
            [FromServices] IAdminService adminService,
            string? search,
            UserRole? role,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await adminService.ListUsersAsync(search, role, page, pageSize, ct);
            return Results.Ok(new ApiResponse<object>(result, "User list retrieved"));
        })
        .WithName("ListUsers")
        .WithSummary("List users with pagination (admin only)")
        .WithOpenApi();

        // PUT /api/admin/users/{id}/role - Update user role
        group.MapPut("/users/{id:guid}/role", async (
            Guid id,
            [FromServices] IAdminService adminService,
            [FromBody] UpdateUserRoleRequest request,
            CancellationToken ct = default) =>
        {
            var success = await adminService.UpdateUserRoleAsync(id, request.Role, ct);
            if (!success)
                return Results.NotFound(new ApiResponse<object>(null, "User not found"));

            return Results.Ok(new ApiResponse<object>(new { id, role = request.Role.ToString() }, "User role updated"));
        })
        .WithName("UpdateUserRole")
        .WithSummary("Update user role (admin only)")
        .WithOpenApi();

        // GET /api/admin/events - List all events
        group.MapGet("/events", async (
            [FromServices] IAdminService adminService,
            string? search,
            EventStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await adminService.ListAllEventsAsync(search, status, page, pageSize, ct);
            return Results.Ok(new ApiResponse<object>(result, "Event list retrieved"));
        })
        .WithName("ListAllEvents")
        .WithSummary("List all events with pagination (admin only)")
        .WithOpenApi();

        // PUT /api/admin/events/{id}/status - Update event status
        group.MapPut("/events/{id:guid}/status", async (
            Guid id,
            [FromServices] IAdminService adminService,
            [FromBody] UpdateEventStatusRequest request,
            CancellationToken ct = default) =>
        {
            if (!Enum.TryParse<EventStatus>(request.Status, out var status))
                return Results.BadRequest(new ApiResponse<object>(null, "Invalid status"));

            var success = await adminService.UpdateEventStatusAsync(id, status, request.Reason, ct);
            if (!success)
                return Results.NotFound(new ApiResponse<object>(null, "Event not found"));

            return Results.Ok(new ApiResponse<object>(new { id, status }, "Event status updated"));
        })
        .WithName("UpdateEventStatus")
        .WithSummary("Update event status (admin only)")
        .WithOpenApi();

        // GET /api/admin/transactions - List transactions
        group.MapGet("/transactions", async (
            [FromServices] IAdminService adminService,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await adminService.ListTransactionsAsync(page, pageSize, ct);
            return Results.Ok(new ApiResponse<object>(result, "Transactions retrieved"));
        })
        .WithName("ListTransactions")
        .WithSummary("List payment transactions (admin only)")
        .WithOpenApi();
    }
}

public record UpdateUserRoleRequest(UserRole Role);
public record UpdateEventStatusRequest(string Status, string? Reason);
