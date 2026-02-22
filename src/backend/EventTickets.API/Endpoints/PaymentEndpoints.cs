using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventTickets.API.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments")
            .WithOpenApi();

        // POST /api/payments/sepay/webhook - SePay payment callback
        group.MapPost("/sepay/webhook", async (
            [FromBody] SePayWebhookRequest request,
            [FromServices] IOrderRepository orderRepo,
            CancellationToken ct) =>
        {
            // TODO: Verify SePay webhook signature
            // TODO: Find order by payment code
            // TODO: Validate amount matches
            // TODO: Update order status
            // TODO: Generate tickets
            // TODO: Send confirmation email

            return Results.Ok(new WebhookResponse(true, "Webhook received - processing"));
        })
        .WithName("SePayWebhook")
        .WithSummary("SePay payment callback webhook")
        .WithOpenApi();

        // GET /api/payments/methods - Get available payment methods
        group.MapGet("/methods", () =>
        {
            var methods = new[]
            {
                new { Id = "sepay", Name = "SePay QR", Description = "Scan QR code to pay", Enabled = true },
                new { Id = "card", Name = "Credit/Debit Card", Description = "Pay with card", Enabled = false }
            };

            return Results.Ok(new ApiResponse<object>(methods));
        })
        .WithName("GetPaymentMethods")
        .WithSummary("Get available payment methods");
    }
}
