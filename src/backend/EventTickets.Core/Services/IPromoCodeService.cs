namespace EventTickets.Core.Services;

public interface IPromoCodeService
{
    Task<decimal> ValidateAndCalculateDiscountAsync(string code, Guid? eventId, decimal originalAmount, CancellationToken ct = default);
    Task<bool> IsValidPromoCodeAsync(string code, Guid? eventId, CancellationToken ct = default);
    Task IncrementUsageAsync(string code, CancellationToken ct = default);
}
