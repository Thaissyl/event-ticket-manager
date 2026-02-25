using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Enums;

namespace EventTickets.Core.Services;

public interface IPaymentService
{
    /// <summary>
    /// Creates a payment record for an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="amount">Payment amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created payment record with QR code</returns>
    Task<PaymentResponse> CreatePaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes SePay webhook payload
    /// </summary>
    /// <param name="request">Webhook payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processing result</returns>
    Task<PaymentProcessResult> ProcessWebhookAsync(SePayWebhookRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payment status by order ID
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment status response</returns>
    Task<PaymentStatusResponse?> GetPaymentStatusAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels pending payments that have timed out
    /// </summary>
    /// <param name="timeoutMinutes">Timeout in minutes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of payments cancelled</returns>
    Task<int> CancelExpiredPaymentsAsync(int timeoutMinutes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reconciles pending payments by querying SePay API
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of payments reconciled</returns>
    Task<int> ReconcilePaymentsAsync(CancellationToken cancellationToken = default);
}

public record PaymentResponse(
    Guid PaymentId,
    Guid OrderId,
    string PaymentCode,
    string QrCodeUrl,
    decimal Amount,
    DateTime ExpiresAt
);

public record PaymentProcessResult(
    bool Success,
    string Message,
    Guid? OrderId,
    Guid? PaymentId
);

public record PaymentStatusResponse(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    PaymentStatus Status,
    string PaymentCode,
    DateTime CreatedAt,
    DateTime? PaidAt
);
