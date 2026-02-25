using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;
using EventTickets.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
            [FromHeader(Name = "X-SePay-Api-Key")] string? apiKey,
            [FromServices] IPaymentService paymentService,
            [FromServices] ISePayWebhookRepository webhookRepo,
            [FromServices] IOptions<SePayOptions> options,
            CancellationToken ct) =>
        {
            // Verify webhook API key
            var expectedApiKey = options.Value.WebhookApiKey;
            if (string.IsNullOrEmpty(expectedApiKey) || apiKey != expectedApiKey)
            {
                return Results.Unauthorized();
            }

            // Log webhook for audit
            var webhook = new SePayWebhook
            {
                Id = Guid.NewGuid(),
                SePayTransactionId = long.Parse(request.TransactionCode),
                Payload = System.Text.Json.JsonSerializer.Serialize(request),
                Processed = false,
                CreatedAt = DateTime.UtcNow
            };
            await webhookRepo.AddAsync(webhook, ct);
            await webhookRepo.SaveChangesAsync(ct);

            // Process payment
            var result = await paymentService.ProcessWebhookAsync(request, ct);

            // Update webhook status
            webhook.Processed = result.Success;
            if (!result.Success)
            {
                webhook.ProcessingError = result.Message;
            }
            await webhookRepo.SaveChangesAsync(ct);

            if (result.Success)
            {
                return Results.Ok(new WebhookResponse(true, "Payment processed successfully"));
            }
            else
            {
                return Results.BadRequest(new WebhookResponse(false, result.Message));
            }
        })
        .WithName("SePayWebhook")
        .WithSummary("SePay payment callback webhook")
        .WithOpenApi();

        // GET /api/payments/status/{orderId} - Get payment status
        group.MapGet("/status/{orderId}", async (
            Guid orderId,
            [FromServices] IPaymentService paymentService,
            CancellationToken ct) =>
        {
            var status = await paymentService.GetPaymentStatusAsync(orderId, ct);
            if (status == null)
            {
                return Results.NotFound(new { message = "Payment not found" });
            }

            return Results.Ok(status);
        })
        .WithName("GetPaymentStatus")
        .WithSummary("Get payment status by order ID")
        .WithOpenApi();

        // POST /api/payments/{orderId}/create - Create payment for order
        group.MapPost("/{orderId}/create", async (
            Guid orderId,
            [FromServices] IPaymentService paymentService,
            [FromServices] IOrderRepository orderRepo,
            CancellationToken ct) =>
        {
            var order = await orderRepo.GetByIdAsync(orderId, ct);
            if (order == null)
            {
                return Results.NotFound(new { message = "Order not found" });
            }

            var response = await paymentService.CreatePaymentAsync(orderId, order.TotalAmount, ct);
            return Results.Ok(response);
        })
        .WithName("CreatePayment")
        .WithSummary("Create payment for order with VietQR code")
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
