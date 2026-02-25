using EventTickets.Core.Entities;

namespace EventTickets.Core.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Payment?> GetBySePayTransactionIdAsync(long transactionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPendingPaymentsOlderThanAsync(DateTime cutoff, CancellationToken cancellationToken = default);
}
