using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
using EventTickets.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Infrastructure.Repositories;

public class SePayWebhookRepository : BaseRepository<SePayWebhook>, ISePayWebhookRepository
{
    public SePayWebhookRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<SePayWebhook?> GetBySePayTransactionIdAsync(long transactionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(w => w.SePayTransactionId == transactionId, cancellationToken);
    }

    public async Task<IEnumerable<SePayWebhook>> GetUnprocessedWebhooksAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => !w.Processed)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
