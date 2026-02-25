using EventTickets.Core.Entities;
using EventTickets.Core.Enums;
using EventTickets.Core.Services;
using EventTickets.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Infrastructure.Services;

public class AnalyticsService(ApplicationDbContext _context) : IAnalyticsService
{
    public async Task<EventSummaryResponse> GetEventSummaryAsync(Guid eventId, CancellationToken ct = default)
    {
        var @event = await _context.Events
            .AsNoTracking()
            .Where(e => e.Id == eventId)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException($"Event {eventId} not found");

        var paidOrders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Tickets.Any(t => t.TicketTier.EventId == eventId))
            .Where(o => o.Status == OrderStatus.Paid)
            .ToListAsync(ct);

        var tickets = await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Order)
            .Where(t => t.TicketTier.EventId == eventId)
            .Where(t => t.Order != null && t.Order.Status == OrderStatus.Paid)
            .ToListAsync(ct);

        var totalRevenue = paidOrders.Sum(o => o.TotalAmount);
        var ticketsSold = tickets.Count;
        var checkedInCount = tickets.Count(t => t.Status == TicketStatus.Used);
        var ordersCount = paidOrders.Count;
        var avgOrderValue = ordersCount > 0 ? totalRevenue / ordersCount : 0;
        var checkinPercentage = ticketsSold > 0 ? (double)checkedInCount / ticketsSold * 100 : 0;

        return new EventSummaryResponse(
            eventId,
            @event.Title ?? "Untitled Event",
            totalRevenue,
            ticketsSold,
            ordersCount,
            checkedInCount,
            Math.Round(checkinPercentage, 2),
            avgOrderValue
        );
    }

    public async Task<SalesTrendResponse> GetSalesTrendAsync(Guid eventId, DateOnly? startDate, DateOnly? endDate, CancellationToken ct = default)
    {
        var @event = await _context.Events
            .AsNoTracking()
            .Where(e => e.Id == eventId)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException($"Event {eventId} not found");

        var start = startDate ?? DateOnly.FromDateTime(@event.CreatedAt);
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var salesData = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Tickets.Any(t => t.TicketTier.EventId == eventId))
            .Where(o => o.Status == OrderStatus.Paid)
            .Where(o => o.CreatedAt >= start.ToDateTime(TimeOnly.MinValue))
            .Where(o => o.CreatedAt <= end.ToDateTime(TimeOnly.MaxValue))
            .GroupBy(o => DateOnly.FromDateTime(o.CreatedAt))
            .Select(g => new SalesTrendDataPoint(
                g.Key,
                g.Sum(o => o.Tickets.Count(t => t.TicketTier.EventId == eventId)),
                g.Sum(o => o.TotalAmount)
            ))
            .OrderBy(d => d.Date)
            .ToListAsync(ct);

        return new SalesTrendResponse(eventId, salesData);
    }

    public async Task<TierBreakdownResponse> GetTierBreakdownAsync(Guid eventId, CancellationToken ct = default)
    {
        var tiers = await _context.TicketTiers
            .AsNoTracking()
            .Where(tt => tt.EventId == eventId)
            .ToListAsync(ct);

        var result = new List<TierBreakdownItem>();

        foreach (var tier in tiers)
        {
            var sold = await _context.Tickets
                .AsNoTracking()
                .Include(t => t.Order)
                .Where(t => t.TicketTierId == tier.Id)
                .Where(t => t.Order != null && t.Order.Status == OrderStatus.Paid)
                .CountAsync(ct);

            var revenue = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Paid)
                .Where(o => o.Tickets.Any(t => t.TicketTierId == tier.Id))
                .SumAsync(o => o.TotalAmount, ct);

            var percentageSold = tier.QuantityTotal > 0 ? (double)sold / tier.QuantityTotal * 100 : 0;

            result.Add(new TierBreakdownItem(
                tier.Id,
                tier.Name ?? "Unnamed Tier",
                sold,
                tier.QuantityTotal,
                revenue,
                Math.Round(percentageSold, 2)
            ));
        }

        return new TierBreakdownResponse(eventId, result);
    }

    public async Task<PromoCodeStatsResponse> GetPromoCodeStatsAsync(Guid eventId, CancellationToken ct = default)
    {
        var promoCodes = await _context.PromoCodes
            .AsNoTracking()
            .Where(p => p.EventId == eventId)
            .ToListAsync(ct);

        // Note: Order-PromoCode relationship not yet established in database schema
        // Returning current usage count from PromoCode entity
        // Full tracking requires OrderPromoCode join table to be added
        var result = promoCodes.Select(promo => new PromoCodeStatItem(
            promo.Id,
            promo.Code,
            promo.CurrentUses,
            0, // discountAmount - requires join table
            0  // revenueGenerated - requires join table
        )).ToList();

        return new PromoCodeStatsResponse(eventId, result);
    }

    public async Task<RecentOrdersResponse> GetRecentOrdersAsync(Guid eventId, int count, CancellationToken ct = default)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Tickets.Any(t => t.TicketTier.EventId == eventId))
            .OrderByDescending(o => o.CreatedAt)
            .Take(Math.Min(count, 100))
            .Select(o => new RecentOrderItem(
                o.PaymentCode,
                o.UserId != null ? "User" : o.GuestEmail,
                o.TotalAmount,
                o.Status.ToString(),
                o.CreatedAt,
                o.Tickets.Count
            ))
            .ToListAsync(ct);

        return new RecentOrdersResponse(eventId, orders);
    }
}
