using EventTickets.Core.Entities;

namespace EventTickets.Core.Interfaces;

public interface ISePayWebhookRepository : IRepository<SePayWebhook>
{
    Task<SePayWebhook?> GetBySePayTransactionIdAsync(long transactionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SePayWebhook>> GetUnprocessedWebhooksAsync(CancellationToken cancellationToken = default);
}
