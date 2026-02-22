namespace EventTickets.Core.DTOs;

public record SalesDataResponse(
    Guid EventId,
    string EventTitle,
    int TotalTickets,
    int TicketsSold,
    int TicketsCheckedIn,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    IEnumerable<TierSalesData> TierBreakdown
);

public record TierSalesData(
    Guid TierId,
    string TierName,
    int Sold,
    decimal Revenue
);

public record CheckinDataResponse(
    Guid EventId,
    string EventTitle,
    int TotalTickets,
    int CheckedIn,
    double CheckinRate,
    IEnumerable<CheckinTimeSlotData> CheckinsByHour
);

public record CheckinTimeSlotData(
    string HourLabel,
    int Count
);
