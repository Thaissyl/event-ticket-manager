using EventTickets.Core.Enums;

namespace EventTickets.Core.DTOs;

public record UserSummaryResponse(
    Guid Id,
    string Email,
    DateTime CreatedAt,
    int OrderCount
);

public record AdminEventSummaryResponse(
    Guid Id,
    string Title,
    string OrganizerEmail,
    EventStatus Status,
    DateTime StartDateTime,
    int TicketsSold,
    decimal Revenue
);

public record PlatformStatsResponse(
    int TotalUsers,
    int TotalEvents,
    int PublishedEvents,
    int TotalOrders,
    decimal TotalRevenue,
    int ActiveEvents
);
