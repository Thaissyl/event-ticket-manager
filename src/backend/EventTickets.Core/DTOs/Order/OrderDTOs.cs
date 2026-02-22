using EventTickets.Core.Entities;
using EventTickets.Core.Enums;

namespace EventTickets.Core.DTOs;

public record TicketResponse(
    Guid Id,
    Guid TicketTierId,
    string TierName,
    string AttendeeName,
    string AttendeeEmail,
    string QrCode,
    TicketStatus Status,
    DateTime? CheckedInAt
);

public record OrderResponse(
    Guid Id,
    string GuestName,
    string GuestEmail,
    decimal TotalAmount,
    OrderStatus Status,
    string PaymentCode,
    DateTime CreatedAt,
    IEnumerable<TicketResponse> Tickets
);

public record CreateOrderRequest(
    string GuestName,
    string GuestEmail,
    IEnumerable<CreateOrderItemRequest> Items
);

public record CreateOrderItemRequest(
    Guid TicketTierId,
    int Quantity
);
