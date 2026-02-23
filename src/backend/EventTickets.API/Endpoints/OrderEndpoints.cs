using EventTickets.Core.DTOs;
using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;
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
            HttpContext httpContext,
            [FromServices] IOrderService orderService,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var orders = await orderService.GetUserOrdersAsync(userId, ct);
            return Results.Ok(orders);
        })
        .WithName("GetOrders")
        .WithSummary("Get user's orders")
        .RequireAuthorization();

        // GET /api/orders/{id} - Get order details
        group.MapGet("/{id:guid}", async (
            Guid id,
            [FromServices] IOrderService orderService,
            CancellationToken ct) =>
        {
            var order = await orderService.GetOrderAsync(id, ct);
            if (order is null)
                return Results.NotFound(new ApiError("NOT_FOUND", "Order not found"));

            return Results.Ok(order);
        })
        .WithName("GetOrderById")
        .WithSummary("Get order by ID");

        // POST /api/orders - Create order from cart
        group.MapPost("", async (
            [FromBody] CreateOrderRequest request,
            HttpContext httpContext,
            [FromServices] IOrderService orderService,
            CancellationToken ct) =>
        {
            var sessionId = GetSessionId(httpContext);

            try
            {
                var order = await orderService.CreateOrderFromCartAsync(sessionId, request, ct);

                // Clear the session cookie after successful order
                ClearSessionCookie(httpContext);

                return Results.Created($"/api/orders/{order.Id}", order);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new ApiError("ORDER_ERROR", ex.Message));
            }
        })
        .WithName("CreateOrder")
        .WithSummary("Create order from cart");

        // GET /api/orders/by-code/{paymentCode} - Get order by payment code (for SePay webhook)
        group.MapGet("/by-code/{paymentCode}", async (
            string paymentCode,
            [FromServices] IOrderService orderService,
            CancellationToken ct) =>
        {
            var order = await orderService.GetOrderByPaymentCodeAsync(paymentCode, ct);
            if (order is null)
                return Results.NotFound(new ApiError("NOT_FOUND", "Order not found"));

            return Results.Ok(order);
        })
        .WithName("GetOrderByPaymentCode")
        .WithSummary("Get order by payment code");
    }

    private static string GetSessionId(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue("cart_session", out var sessionId))
            return sessionId!;

        return Guid.NewGuid().ToString();
    }

    private static void ClearSessionCookie(HttpContext context)
    {
        context.Response.Cookies.Delete("cart_session");
    }
}
