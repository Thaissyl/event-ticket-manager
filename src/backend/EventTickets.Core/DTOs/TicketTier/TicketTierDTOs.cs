namespace EventTickets.Core.DTOs;

public record TicketTierResponse(
    Guid Id,
    Guid EventId,
    string Name,
    string? Description,
    decimal Price,
    int QuantityTotal,
    int QuantitySold,
    int QuantityAvailable,
    DateTime SaleStartDateTime,
    DateTime SaleEndDateTime
);

public record CreateTicketTierRequest(
    Guid EventId,
    string Name,
    string? Description,
    decimal Price,
    int QuantityTotal,
    DateTime SaleStartDateTime,
    DateTime SaleEndDateTime
);

public record UpdateTicketTierRequest(
    string Name,
    string? Description,
    decimal Price,
    int QuantityTotal,
    DateTime SaleStartDateTime,
    DateTime SaleEndDateTime
);
