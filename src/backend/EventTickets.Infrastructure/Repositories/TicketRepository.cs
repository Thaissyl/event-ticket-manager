using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
using EventTickets.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Infrastructure.Repositories;

public class TicketRepository : BaseRepository<Ticket>, ITicketRepository
{
    public TicketRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Ticket?> GetWithTicketTierAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.TicketTier)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Ticket?> GetWithTicketTierAndOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.TicketTier)
            .ThenInclude(tt => tt!.Event)
            .Include(t => t.Order)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.TicketTier)
            .Where(t => t.TicketTier!.EventId == eventId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Ticket?> GetByQrCodeAsync(string qrCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.QrCode == qrCode, cancellationToken);
    }
}
