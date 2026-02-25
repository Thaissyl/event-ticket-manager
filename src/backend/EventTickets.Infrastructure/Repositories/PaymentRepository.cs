using EventTickets.Core.Entities;
using EventTickets.Core.Enums;
using EventTickets.Core.Interfaces;
using EventTickets.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Infrastructure.Repositories;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
    }

    public async Task<Payment?> GetBySePayTransactionIdAsync(long transactionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.SePayTransactionId == transactionId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPendingPaymentsOlderThanAsync(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Order)
            .Where(p => p.Status == PaymentStatus.Pending && p.CreatedAt < cutoff)
            .ToListAsync(cancellationToken);
    }
}
