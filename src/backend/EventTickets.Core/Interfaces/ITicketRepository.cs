using EventTickets.Core.Entities;

namespace EventTickets.Core.Interfaces;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<Ticket?> GetWithTicketTierAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Ticket?> GetWithTicketTierAndOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByQrCodeAsync(string qrCode, CancellationToken cancellationToken = default);
}
