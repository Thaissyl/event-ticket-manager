using EventTickets.Core.Entities;

namespace EventTickets.Core.Interfaces;

public interface ITicketTierRepository : IRepository<TicketTier>
{
    Task<TicketTier?> GetWithTicketsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TicketTier>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}
