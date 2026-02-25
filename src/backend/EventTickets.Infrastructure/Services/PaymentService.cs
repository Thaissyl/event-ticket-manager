using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Enums;
using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventTickets.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ISePayWebhookRepository _webhookRepository;
    private readonly IVietQrService _vietQrService;
    private readonly SePayOptions _options;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        ISePayWebhookRepository webhookRepository,
        IVietQrService vietQrService,
        IOptions<SePayOptions> options,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _webhookRepository = webhookRepository;
        _vietQrService = vietQrService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PaymentResponse> CreatePaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByPaymentCodeAsync("", cancellationToken);
        order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Order {orderId} is not in pending status");

        var paymentCode = order.PaymentCode;
        var qrResponse = await _vietQrService.GenerateQrCodeAsync(amount, paymentCode, cancellationToken);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            ReferenceCode = paymentCode,
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        return new PaymentResponse(
            payment.Id,
            payment.OrderId,
            paymentCode,
            qrResponse.QrCodeUrl,
            amount,
            DateTime.UtcNow.AddMinutes(30)
        );
    }

    public async Task<PaymentProcessResult> ProcessWebhookAsync(SePayWebhookRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing SePay webhook: {TransactionCode}, Amount: {Amount}, Content: {Content}",
                request.TransactionCode, request.Amount, request.Content);

            // Check idempotency - already processed this transaction?
            var existingPayment = await _paymentRepository.GetBySePayTransactionIdAsync(long.Parse(request.TransactionCode), cancellationToken);
            if (existingPayment != null)
            {
                _logger.LogInformation("Transaction {TransactionCode} already processed", request.TransactionCode);
                return new PaymentProcessResult(true, "Transaction already processed", existingPayment.OrderId, existingPayment.Id);
            }

            // Extract payment code from content
            var paymentCode = ExtractPaymentCode(request.Content ?? "");
            if (string.IsNullOrEmpty(paymentCode))
            {
                _logger.LogWarning("Could not extract payment code from content: {Content}", request.Content);
                return new PaymentProcessResult(false, "Invalid payment code in content", null, null);
            }

            // Find order by payment code
            var order = await _orderRepository.GetByPaymentCodeAsync(paymentCode, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Order not found for payment code: {PaymentCode}", paymentCode);
                return new PaymentProcessResult(false, $"Order not found for payment code: {paymentCode}", null, null);
            }

            // Validate amount
            if (Math.Abs(request.Amount - order.TotalAmount) > 0.01m)
            {
                _logger.LogWarning("Amount mismatch for order {OrderId}: expected {Expected}, received {Received}",
                    order.Id, order.TotalAmount, request.Amount);
                return new PaymentProcessResult(false, $"Amount mismatch: expected {order.TotalAmount}, received {request.Amount}", null, null);
            }

            // Check if order is still pending
            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogInformation("Order {OrderId} is not in pending status: {Status}", order.Id, order.Status);
                return new PaymentProcessResult(false, $"Order is not in pending status: {order.Status}", null, null);
            }

            // Get or create payment record
            var payment = await _paymentRepository.GetByOrderIdAsync(order.Id, cancellationToken);
            if (payment == null)
            {
                payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Amount = request.Amount,
                    Status = PaymentStatus.Pending,
                    ReferenceCode = paymentCode,
                    CreatedAt = DateTime.UtcNow
                };
                await _paymentRepository.AddAsync(payment, cancellationToken);
            }

            // Update payment status
            payment.SePayTransactionId = long.Parse(request.TransactionCode);
            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = request.TransactionDate;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            // Update order status
            order.Status = OrderStatus.Paid;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order, cancellationToken);

            // Update tickets status to Valid
            foreach (var ticket in order.Tickets)
            {
                ticket.Status = TicketStatus.Valid;
            }

            await _paymentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully processed payment for order {OrderId}", order.Id);

            return new PaymentProcessResult(true, "Payment processed successfully", order.Id, payment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook: {Message}", ex.Message);
            return new PaymentProcessResult(false, $"Error processing webhook: {ex.Message}", null, null);
        }
    }

    public async Task<PaymentStatusResponse?> GetPaymentStatusAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        if (payment == null)
            return null;

        return new PaymentStatusResponse(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            payment.Status,
            payment.ReferenceCode,
            payment.CreatedAt,
            payment.PaidAt
        );
    }

    public async Task<int> CancelExpiredPaymentsAsync(int timeoutMinutes, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-timeoutMinutes);
        var expiredPayments = await _paymentRepository.GetPendingPaymentsOlderThanAsync(cutoff, cancellationToken);

        var cancelledCount = 0;
        foreach (var payment in expiredPayments)
        {
            var order = payment.Order;
            if (order != null && order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;

                payment.Status = PaymentStatus.Failed;

                // Cancel tickets
                foreach (var ticket in order.Tickets)
                {
                    ticket.Status = TicketStatus.Cancelled;
                }

                cancelledCount++;
                _logger.LogInformation("Cancelled expired payment {PaymentId} for order {OrderId}", payment.Id, order.Id);
            }
        }

        if (cancelledCount > 0)
        {
            await _paymentRepository.SaveChangesAsync(cancellationToken);
        }

        return cancelledCount;
    }

    public Task<int> ReconcilePaymentsAsync(CancellationToken cancellationToken = default)
    {
        // This would call SePay API to check for missed transactions
        // For now, return 0 as this requires API implementation
        _logger.LogInformation("Payment reconciliation not yet implemented with SePay API");
        return Task.FromResult(0);
    }

    private string ExtractPaymentCode(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        // Try to find ETM-XXXX pattern
        var match = System.Text.RegularExpressions.Regex.Match(content, @"ETM-[\dA-Z-]+");
        return match.Success ? match.Value : string.Empty;
    }
}
