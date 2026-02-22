using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventTickets.API.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrders(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        // GET /api/orders - Get user's orders
        group.MapGet("", async (
            [FromServices] IOrderRepository repo,
            CancellationToken ct) =>
        {
            // TODO: Implement with user context from Phase 04
            var orders = await repo.GetAllAsync(ct);
            return Results.Ok(orders);
        })
        .WithName("GetOrders")
        .WithSummary("Get user's orders")
        .RequireAuthorization();

        // GET /api/orders/{id} - Get order details
        group.MapGet("/{id:guid}", async (
            Guid id,
            [FromServices] IOrderRepository repo,
            CancellationToken ct) =>
        {
            var order = await repo.GetWithTicketsAsync(id, ct);
            if (order is null)
                return Results.NotFound();

            return Results.Ok(order);
        })
        .WithName("GetOrderById")
        .WithSummary("Get order by ID")
        .RequireAuthorization();
    }
}
