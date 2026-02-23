using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Enums;
using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITicketTierRepository _ticketTierRepository;
    private readonly ICartService _cartService;

    public OrderService(
        IOrderRepository orderRepository,
        ITicketTierRepository ticketTierRepository,
        ICartService cartService)
    {
        _orderRepository = orderRepository;
        _ticketTierRepository = ticketTierRepository;
        _cartService = cartService;
    }

    public async Task<OrderResponse> CreateOrderFromCartAsync(string sessionId, CreateOrderRequest request, CancellationToken ct = default)
    {
        // Get cart items
        var cartItems = await _cartService.GetCartItemsForOrderAsync(sessionId, ct);
        if (!cartItems.Any())
            throw new InvalidOperationException("Cart is empty");

        // Validate guest information
        if (string.IsNullOrWhiteSpace(request.GuestName) || string.IsNullOrWhiteSpace(request.GuestEmail))
            throw new InvalidOperationException("Guest name and email are required");

        // Calculate total and create order
        decimal totalAmount = 0;
        var tickets = new List<Ticket>();

        foreach (var (tierId, quantity) in cartItems)
        {
            var tier = await _ticketTierRepository.GetWithTicketsAsync(tierId, ct);
            if (tier == null)
                throw new InvalidOperationException($"Ticket tier {tierId} not found");

            totalAmount += tier.Price * quantity;

            // Create tickets for each quantity
            for (int i = 0; i < quantity; i++)
            {
                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    TicketTierId = tierId,
                    QrCode = GenerateQrCode(),
                    QrCodeSignature = GenerateQrSignature(),
                    AttendeeName = request.GuestName,
                    AttendeeEmail = request.GuestEmail,
                    Status = TicketStatus.Valid,
                    CreatedAt = DateTime.UtcNow
                };
                tickets.Add(ticket);
            }

            // Update tier sold quantity
            tier.QuantitySold += quantity;
            await _ticketTierRepository.UpdateAsync(tier, ct);
        }

        // Generate unique payment code for SePay
        var paymentCode = GeneratePaymentCode();

        // Create order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            GuestName = request.GuestName,
            GuestEmail = request.GuestEmail,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            PaymentCode = paymentCode,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Tickets = tickets
        };

        await _orderRepository.AddAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);

        // Clear cart after successful order creation
        await _cartService.ReleaseCartReservationsAsync(sessionId, ct);

        // Build response
        var ticketResponses = tickets.Select(t => new TicketResponse(
            t.Id,
            t.TicketTierId,
            "", // TierName will be loaded via navigation
            t.AttendeeName,
            t.AttendeeEmail,
            t.QrCode,
            t.Status,
            t.CheckedInAt
        ));

        return new OrderResponse(
            order.Id,
            order.GuestName,
            order.GuestEmail,
            order.TotalAmount,
            order.Status,
            order.PaymentCode,
            order.CreatedAt,
            ticketResponses
        );
    }

    public async Task<OrderResponse?> GetOrderAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetWithTicketsAsync(id, ct);
        if (order == null)
            return null;

        var ticketResponses = order.Tickets.Select(t => new TicketResponse(
            t.Id,
            t.TicketTierId,
            t.TicketTier?.Name ?? "",
            t.AttendeeName,
            t.AttendeeEmail,
            t.QrCode,
            t.Status,
            t.CheckedInAt
        ));

        return new OrderResponse(
            order.Id,
            order.GuestName,
            order.GuestEmail,
            order.TotalAmount,
            order.Status,
            order.PaymentCode,
            order.CreatedAt,
            ticketResponses
        );
    }

    public async Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(Guid userId, CancellationToken ct = default)
    {
        var orders = await _orderRepository.GetByUserAsync(userId, ct);

        return orders.Select(o => new OrderResponse(
            o.Id,
            o.GuestName,
            o.GuestEmail,
            o.TotalAmount,
            o.Status,
            o.PaymentCode,
            o.CreatedAt,
            o.Tickets.Select(t => new TicketResponse(
                t.Id,
                t.TicketTierId,
                t.TicketTier?.Name ?? "",
                t.AttendeeName,
                t.AttendeeEmail,
                t.QrCode,
                t.Status,
                t.CheckedInAt
            ))
        ));
    }

    public async Task<OrderResponse?> GetOrderByPaymentCodeAsync(string paymentCode, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByPaymentCodeAsync(paymentCode, ct);
        if (order == null)
            return null;

        return await GetOrderAsync(order.Id, ct);
    }

    private string GeneratePaymentCode()
    {
        // Generate a unique 8-character payment code
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        var code = new char[8];

        for (int i = 0; i < 8; i++)
        {
            code[i] = chars[random.Next(chars.Length)];
        }

        return new string(code);
    }

    private string GenerateQrCode()
    {
        // Generate unique QR code
        return Guid.NewGuid().ToString("N");
    }

    private string GenerateQrSignature()
    {
        // Generate signature for QR code validation
        return Guid.NewGuid().ToString("N");
    }
}
