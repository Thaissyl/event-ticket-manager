using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;

namespace EventTickets.Core.Services;

public interface IEventService
{
    Task<Event> CreateEventAsync(CreateEventRequest request, Guid organizerId, CancellationToken ct = default);
    Task<Event?> UpdateEventAsync(Guid id, UpdateEventRequest request, Guid organizerId, CancellationToken ct = default);
    Task<bool> DeleteEventAsync(Guid id, Guid organizerId, CancellationToken ct = default);
    Task<bool> PublishEventAsync(Guid id, Guid organizerId, CancellationToken ct = default);
    Task<bool> CancelEventAsync(Guid id, Guid organizerId, CancellationToken ct = default);
    Task<bool> CompleteEventAsync(Guid id, Guid organizerId, CancellationToken ct = default);
    Task<bool> CanUserModifyEventAsync(Guid eventId, Guid userId, CancellationToken ct = default);
}
