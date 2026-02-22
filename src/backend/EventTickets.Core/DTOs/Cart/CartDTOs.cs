namespace EventTickets.Core.DTOs;

public record CartItemResponse(
    Guid TicketTierId,
    string TierName,
    decimal Price,
    int Quantity,
    decimal Subtotal
);

public record AddToCartRequest(
    Guid TicketTierId,
    int Quantity
);

public record UpdateCartItemRequest(
    int Quantity
);

public record CartResponse(
    IEnumerable<CartItemResponse> Items,
    int TotalItems,
    decimal TotalAmount
);
