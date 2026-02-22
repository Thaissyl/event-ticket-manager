using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
using EventTickets.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Infrastructure.Repositories;

public class TicketTierRepository : BaseRepository<TicketTier>, ITicketTierRepository
{
    public TicketTierRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<TicketTier?> GetWithTicketsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Tickets)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TicketTier>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.EventId == eventId)
            .OrderBy(t => t.Price)
            .ToListAsync(cancellationToken);
    }
}
