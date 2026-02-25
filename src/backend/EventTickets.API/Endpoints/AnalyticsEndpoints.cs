using EventTickets.Core.DTOs;
using EventTickets.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventTickets.API.Endpoints;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics")
            .WithTags("Analytics")
            .WithOpenApi();

        // GET /api/analytics/events/{id}/summary - Get event summary
        group.MapGet("/events/{id:guid}/summary", async (
            Guid id,
            [FromServices] IAnalyticsService analyticsService,
            CancellationToken ct) =>
        {
            var summary = await analyticsService.GetEventSummaryAsync(id, ct);
            return Results.Ok(new ApiResponse<object>(summary, "Event summary retrieved"));
        })
        .WithName("GetEventSummary")
        .WithSummary("Get summary analytics for an event")
        .WithOpenApi();

        // GET /api/analytics/events/{id}/sales-trend - Get sales over time
        group.MapGet("/events/{id:guid}/sales-trend", async (
            Guid id,
            DateOnly? startDate,
            DateOnly? endDate,
            [FromServices] IAnalyticsService analyticsService,
            CancellationToken ct) =>
        {
            var trend = await analyticsService.GetSalesTrendAsync(id, startDate, endDate, ct);
            return Results.Ok(new ApiResponse<object>(trend, "Sales trend retrieved"));
        })
        .WithName("GetSalesTrend")
        .WithSummary("Get sales trend over time for an event")
        .WithOpenApi();

        // GET /api/analytics/events/{id}/tier-breakdown - Get sales by tier
        group.MapGet("/events/{id:guid}/tier-breakdown", async (
            Guid id,
            [FromServices] IAnalyticsService analyticsService,
            CancellationToken ct) =>
        {
            var breakdown = await analyticsService.GetTierBreakdownAsync(id, ct);
            return Results.Ok(new ApiResponse<object>(breakdown, "Tier breakdown retrieved"));
        })
        .WithName("GetTierBreakdown")
        .WithSummary("Get ticket tier breakdown for an event")
        .WithOpenApi();

        // GET /api/analytics/events/{id}/promo-stats - Get promo code stats
        group.MapGet("/events/{id:guid}/promo-stats", async (
            Guid id,
            [FromServices] IAnalyticsService analyticsService,
            CancellationToken ct) =>
        {
            var stats = await analyticsService.GetPromoCodeStatsAsync(id, ct);
            return Results.Ok(new ApiResponse<object>(stats, "Promo code stats retrieved"));
        })
        .WithName("GetPromoStats")
        .WithSummary("Get promo code effectiveness for an event")
        .WithOpenApi();

        // GET /api/analytics/events/{id}/recent-orders - Get recent orders
        group.MapGet("/events/{id:guid}/recent-orders", async (
            Guid id,
            [FromServices] IAnalyticsService analyticsService,
            [FromQuery] int count = 10,
            CancellationToken ct = default) =>
        {
            var orders = await analyticsService.GetRecentOrdersAsync(id, count, ct);
            return Results.Ok(new ApiResponse<object>(orders, "Recent orders retrieved"));
        })
        .WithName("GetRecentOrders")
        .WithSummary("Get recent orders for an event")
        .WithOpenApi();

        // GET /api/analytics/events/{id}/export/attendees - Export attendees as CSV
        group.MapGet("/events/{id:guid}/export/attendees", async (
            Guid id,
            [FromServices] IExportService exportService,
            CancellationToken ct) =>
        {
            var csvBytes = await exportService.ExportAttendeesAsync(id, ct);
            return Results.File(csvBytes, "text/csv", $"attendees-{id}.csv");
        })
        .WithName("ExportAttendees")
        .WithSummary("Export attendees list as CSV")
        .WithOpenApi();

        // GET /api/analytics/events/{id}/export/sales - Export sales report as CSV
        group.MapGet("/events/{id:guid}/export/sales", async (
            Guid id,
            [FromServices] IExportService exportService,
            CancellationToken ct) =>
        {
            var csvBytes = await exportService.ExportSalesReportAsync(id, ct);
            return Results.File(csvBytes, "text/csv", $"sales-report-{id}.csv");
        })
        .WithName("ExportSalesReport")
        .WithSummary("Export sales report as CSV")
        .WithOpenApi();
    }
}
