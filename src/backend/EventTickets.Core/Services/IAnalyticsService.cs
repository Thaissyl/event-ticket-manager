namespace EventTickets.Core.Services;

public interface IAnalyticsService
{
    Task<EventSummaryResponse> GetEventSummaryAsync(Guid eventId, CancellationToken ct = default);
    Task<SalesTrendResponse> GetSalesTrendAsync(Guid eventId, DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<TierBreakdownResponse> GetTierBreakdownAsync(Guid eventId, CancellationToken ct = default);
    Task<PromoCodeStatsResponse> GetPromoCodeStatsAsync(Guid eventId, CancellationToken ct = default);
    Task<RecentOrdersResponse> GetRecentOrdersAsync(Guid eventId, int count = 10, CancellationToken ct = default);
}

// Response DTOs
public record EventSummaryResponse(
    Guid EventId,
    string EventTitle,
    decimal TotalRevenue,
    int TicketsSold,
    int OrdersCount,
    int CheckedInCount,
    double CheckedInPercentage,
    decimal AverageOrderValue
);

public record SalesTrendResponse(
    Guid EventId,
    IEnumerable<SalesTrendDataPoint> Data
);

public record SalesTrendDataPoint(
    DateOnly Date,
    int TicketsSold,
    decimal Revenue
);

public record TierBreakdownResponse(
    Guid EventId,
    IEnumerable<TierBreakdownItem> Tiers
);

public record TierBreakdownItem(
    Guid Id,
    string Name,
    int Sold,
    int Total,
    decimal Revenue,
    double PercentageSold
);

public record PromoCodeStatsResponse(
    Guid EventId,
    IEnumerable<PromoCodeStatItem> PromoCodes
);

public record PromoCodeStatItem(
    Guid Id,
    string Code,
    int Uses,
    decimal DiscountAmount,
    decimal RevenueGenerated
);

public record RecentOrdersResponse(
    Guid EventId,
    IEnumerable<RecentOrderItem> Orders
);

public record RecentOrderItem(
    string OrderCode,
    string CustomerEmail,
    decimal Amount,
    string Status,
    DateTime CreatedAt,
    int TicketCount
);
