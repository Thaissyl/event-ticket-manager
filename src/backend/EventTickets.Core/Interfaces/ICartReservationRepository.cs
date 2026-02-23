using EventTickets.Core.Entities;

namespace EventTickets.Core.Interfaces;

public interface ICartReservationRepository : IRepository<CartReservation>
{
    Task<CartReservation?> GetBySessionAndTierAsync(string sessionId, Guid ticketTierId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CartReservation>> GetBySessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CartReservation>> GetExpiredAsync(CancellationToken cancellationToken = default);
    Task<int> GetTotalReservedForTierAsync(Guid ticketTierId, CancellationToken cancellationToken = default);
}
