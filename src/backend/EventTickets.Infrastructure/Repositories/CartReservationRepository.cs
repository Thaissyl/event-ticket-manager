using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
using EventTickets.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Infrastructure.Repositories;

public class CartReservationRepository : BaseRepository<CartReservation>, ICartReservationRepository
{
    public CartReservationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<CartReservation?> GetBySessionAndTierAsync(string sessionId, Guid ticketTierId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(cr => cr.SessionId == sessionId && cr.TicketTierId == ticketTierId && cr.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    public async Task<IEnumerable<CartReservation>> GetBySessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cr => cr.TicketTier)
            .Where(cr => cr.SessionId == sessionId && cr.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CartReservation>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cr => cr.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalReservedForTierAsync(Guid ticketTierId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cr => cr.TicketTierId == ticketTierId && cr.ExpiresAt > DateTime.UtcNow)
            .SumAsync(cr => cr.Quantity, cancellationToken);
    }
}
