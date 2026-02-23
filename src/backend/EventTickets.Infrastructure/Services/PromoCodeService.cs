using EventTickets.Core.Entities;
using EventTickets.Core.Enums;
using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Infrastructure.Services;

public class PromoCodeService : IPromoCodeService
{
    private readonly IRepository<PromoCode> _promoCodeRepository;
    private readonly IEventRepository _eventRepository;

    public PromoCodeService(
        IRepository<PromoCode> promoCodeRepository,
        IEventRepository eventRepository)
    {
        _promoCodeRepository = promoCodeRepository;
        _eventRepository = eventRepository;
    }

    public async Task<decimal> ValidateAndCalculateDiscountAsync(string code, Guid? eventId, decimal originalAmount, CancellationToken ct = default)
    {
        var promoCode = await FindValidPromoCodeAsync(code, eventId, ct);
        if (promoCode == null)
            return 0;

        return CalculateDiscount(promoCode, originalAmount);
    }

    public async Task<bool> IsValidPromoCodeAsync(string code, Guid? eventId, CancellationToken ct = default)
    {
        var promoCode = await FindValidPromoCodeAsync(code, eventId, ct);
        return promoCode != null;
    }

    public async Task IncrementUsageAsync(string code, CancellationToken ct = default)
    {
        var promoCodes = await _promoCodeRepository.FindAsync(
            p => p.Code == code.ToUpper(),
            ct
        );

        var promoCode = promoCodes.FirstOrDefault();
        if (promoCode == null)
            return;

        promoCode.CurrentUses++;
        await _promoCodeRepository.UpdateAsync(promoCode, ct);
        await _promoCodeRepository.SaveChangesAsync(ct);
    }

    private async Task<PromoCode?> FindValidPromoCodeAsync(string code, Guid? eventId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var normalizedCode = code.ToUpper().Trim();
        var now = DateTime.UtcNow;

        var promoCodes = await _promoCodeRepository.FindAsync(
            p => p.Code == normalizedCode,
            ct
        );

        var promoCode = promoCodes.FirstOrDefault();
        if (promoCode == null)
            return null;

        // Check date validity
        if (now < promoCode.ValidFrom || now > promoCode.ValidUntil)
            return null;

        // Check usage limit
        if (promoCode.MaxUses > 0 && promoCode.CurrentUses >= promoCode.MaxUses)
            return null;

        // Check event restriction (if promo code is event-specific)
        if (promoCode.EventId.HasValue)
        {
            if (!eventId.HasValue || promoCode.EventId.Value != eventId.Value)
                return null;
        }

        return promoCode;
    }

    private static decimal CalculateDiscount(PromoCode promoCode, decimal originalAmount)
    {
        return promoCode.DiscountType switch
        {
            DiscountType.Percentage => Math.Round(originalAmount * (promoCode.DiscountValue / 100), 2),
            DiscountType.FixedAmount => Math.Min(promoCode.DiscountValue, originalAmount),
            _ => 0
        };
    }
}
