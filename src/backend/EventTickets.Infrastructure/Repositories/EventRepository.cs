using EventTickets.Core.Entities;
using EventTickets.Core.Enums;
using EventTickets.Core.Interfaces;
using EventTickets.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Infrastructure.Repositories;

public class EventRepository : BaseRepository<Event>, IEventRepository
{
    public EventRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Event?> GetWithTiersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.TicketTiers)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.OrganizerId == organizerId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetPublishedEventsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.Status == EventStatus.Published)
            .Where(e => e.StartDateTime > DateTime.UtcNow)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<Event>();

        // Limit query length to prevent DoS
        var searchTerm = query.Length > 200 ? query.Substring(0, 200).ToLower() : query.ToLower();

        return await _dbSet
            .Where(e => e.Status == EventStatus.Published)
            .Where(e => (e.Title != null && e.Title.ToLower().Contains(searchTerm)) ||
                        (e.Description != null && e.Description.ToLower().Contains(searchTerm)) ||
                        (e.VenueCity != null && e.VenueCity.ToLower().Contains(searchTerm)))
            .OrderBy(e => e.StartDateTime)
            .ToListAsync(cancellationToken);
    }
}
