using EventTickets.Core.Entities;

namespace EventTickets.Core.Interfaces;

public interface IEventRepository : IRepository<Event>
{
    Task<Event?> GetWithTiersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetPublishedEventsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
