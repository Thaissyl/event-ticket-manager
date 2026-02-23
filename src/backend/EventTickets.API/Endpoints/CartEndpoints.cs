using EventTickets.Core.DTOs;
using EventTickets.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventTickets.API.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cart")
            .WithTags("Cart")
            .WithOpenApi();

        // GET /api/cart - Get cart (session-based)
        group.MapGet("", async (
            HttpContext httpContext,
            [FromServices] ICartService cartService,
            CancellationToken ct) =>
        {
            var sessionId = GetSessionId(httpContext);
            var cart = await cartService.GetCartAsync(sessionId, ct);
            return Results.Ok(cart);
        })
        .WithName("GetCart")
        .WithSummary("Get current cart");

        // POST /api/cart/items - Add item to cart
        group.MapPost("/items", async (
            [FromBody] AddToCartRequest request,
            HttpContext httpContext,
            [FromServices] ICartService cartService,
            CancellationToken ct) =>
        {
            var sessionId = GetSessionId(httpContext);

            try
            {
                var cartItem = await cartService.AddItemAsync(sessionId, request.TicketTierId, request.Quantity, ct);
                if (cartItem == null)
                    return Results.NotFound(new ApiError("NOT_FOUND", "Ticket tier not found"));

                SetSessionCookie(httpContext, sessionId);
                return Results.Ok(cartItem);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new ApiError("VALIDATION_ERROR", ex.Message));
            }
        })
        .WithName("AddToCart")
        .WithSummary("Add item to cart");

        // PUT /api/cart/items/{ticketTierId} - Update cart item
        group.MapPut("/items/{ticketTierId:guid}", async (
            Guid ticketTierId,
            [FromBody] UpdateCartItemRequest request,
            HttpContext httpContext,
            [FromServices] ICartService cartService,
            CancellationToken ct) =>
        {
            var sessionId = GetSessionId(httpContext);

            try
            {
                var cartItem = await cartService.UpdateItemAsync(sessionId, ticketTierId, request.Quantity, ct);
                if (cartItem == null)
                    return Results.NotFound(new ApiError("NOT_FOUND", "Item not found in cart"));

                return Results.Ok(cartItem);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new ApiError("VALIDATION_ERROR", ex.Message));
            }
        })
        .WithName("UpdateCartItem")
        .WithSummary("Update cart item quantity");

        // DELETE /api/cart/items/{ticketTierId} - Remove item from cart
        group.MapDelete("/items/{ticketTierId:guid}", async (
            Guid ticketTierId,
            HttpContext httpContext,
            [FromServices] ICartService cartService,
            CancellationToken ct) =>
        {
            var sessionId = GetSessionId(httpContext);
            var removed = await cartService.RemoveItemAsync(sessionId, ticketTierId, ct);

            if (!removed)
                return Results.NotFound(new ApiError("NOT_FOUND", "Item not found in cart"));

            return Results.Ok(new { message = "Item removed from cart" });
        })
        .WithName("RemoveFromCart")
        .WithSummary("Remove item from cart");

        // DELETE /api/cart - Clear cart
        group.MapDelete("", async (
            HttpContext httpContext,
            [FromServices] ICartService cartService,
            CancellationToken ct) =>
        {
            var sessionId = GetSessionId(httpContext);
            await cartService.ClearCartAsync(sessionId, ct);

            ClearSessionCookie(httpContext);
            return Results.Ok(new { message = "Cart cleared" });
        })
        .WithName("ClearCart")
        .WithSummary("Clear all items from cart");
    }

    private static string GetSessionId(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue("cart_session", out var sessionId))
            return sessionId!;

        return Guid.NewGuid().ToString();
    }

    private static void SetSessionCookie(HttpContext context, string sessionId)
    {
        context.Response.Cookies.Append("cart_session", sessionId, new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromDays(30)
        });
    }

    private static void ClearSessionCookie(HttpContext context)
    {
        context.Response.Cookies.Delete("cart_session");
    }
}
