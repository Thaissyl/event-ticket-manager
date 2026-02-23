using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Enums;
using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;

namespace EventTickets.Infrastructure.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly ITicketTierRepository _ticketTierRepository;

    public EventService(
        IEventRepository eventRepository,
        ITicketTierRepository ticketTierRepository)
    {
        _eventRepository = eventRepository;
        _ticketTierRepository = ticketTierRepository;
    }

    public async Task<Event> CreateEventAsync(CreateEventRequest request, Guid organizerId, CancellationToken ct = default)
    {
        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            OrganizerId = organizerId,
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            VenueName = request.VenueName,
            VenueAddress = request.VenueAddress,
            VenueCity = request.VenueCity,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            ImageUrl = request.ImageUrl,
            TotalCapacity = request.TotalCapacity,
            Status = EventStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = 0
        };

        await _eventRepository.AddAsync(eventEntity, ct);
        await _eventRepository.SaveChangesAsync(ct);

        return eventEntity;
    }

    public async Task<Event?> UpdateEventAsync(Guid id, UpdateEventRequest request, Guid organizerId, CancellationToken ct = default)
    {
        var eventEntity = await _eventRepository.GetWithTiersAsync(id, ct);
        if (eventEntity == null)
            return null;

        // Authorization check
        if (eventEntity.OrganizerId != organizerId)
            return null;

        // Prevent updates if event is published (except status changes via dedicated endpoints)
        if (eventEntity.Status == EventStatus.Published || eventEntity.Status == EventStatus.Completed)
        {
            // Only allow limited updates for published events
            eventEntity.Description = request.Description ?? string.Empty;
            eventEntity.ImageUrl = request.ImageUrl;
        }
        else
        {
            eventEntity.Title = request.Title;
            eventEntity.Description = request.Description ?? string.Empty;
            eventEntity.VenueName = request.VenueName;
            eventEntity.VenueAddress = request.VenueAddress;
            eventEntity.VenueCity = request.VenueCity;
            eventEntity.StartDateTime = request.StartDateTime;
            eventEntity.EndDateTime = request.EndDateTime;
            eventEntity.ImageUrl = request.ImageUrl;
            eventEntity.TotalCapacity = request.TotalCapacity;
        }

        eventEntity.UpdatedAt = DateTime.UtcNow;
        await _eventRepository.UpdateAsync(eventEntity, ct);
        await _eventRepository.SaveChangesAsync(ct);

        return eventEntity;
    }

    public async Task<bool> DeleteEventAsync(Guid id, Guid organizerId, CancellationToken ct = default)
    {
        var eventEntity = await _eventRepository.GetWithTiersAsync(id, ct);
        if (eventEntity == null || eventEntity.OrganizerId != organizerId)
            return false;

        // Only allow deletion of draft events
        if (eventEntity.Status != EventStatus.Draft)
            return false;

        // Check if there are any ticket sales
        if (eventEntity.TicketTiers.Any(t => t.QuantitySold > 0))
            return false;

        await _eventRepository.DeleteAsync(eventEntity, ct);
        await _eventRepository.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> PublishEventAsync(Guid id, Guid organizerId, CancellationToken ct = default)
    {
        var eventEntity = await _eventRepository.GetWithTiersAsync(id, ct);
        if (eventEntity == null || eventEntity.OrganizerId != organizerId)
            return false;

        // Can only publish draft events
        if (eventEntity.Status != EventStatus.Draft)
            return false;

        // Must have at least one ticket tier
        if (!eventEntity.TicketTiers.Any())
            return false;

        // Validate event date
        if (eventEntity.StartDateTime <= DateTime.UtcNow)
            return false;

        eventEntity.Status = EventStatus.Published;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        await _eventRepository.UpdateAsync(eventEntity, ct);
        await _eventRepository.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> CancelEventAsync(Guid id, Guid organizerId, CancellationToken ct = default)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(id, ct);
        if (eventEntity == null || eventEntity.OrganizerId != organizerId)
            return false;

        // Can only cancel published events
        if (eventEntity.Status != EventStatus.Published)
            return false;

        eventEntity.Status = EventStatus.Cancelled;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        await _eventRepository.UpdateAsync(eventEntity, ct);
        await _eventRepository.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> CompleteEventAsync(Guid id, Guid organizerId, CancellationToken ct = default)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(id, ct);
        if (eventEntity == null || eventEntity.OrganizerId != organizerId)
            return false;

        // Can only complete published events that have ended
        if (eventEntity.Status != EventStatus.Published)
            return false;

        if (eventEntity.EndDateTime > DateTime.UtcNow)
            return false;

        eventEntity.Status = EventStatus.Completed;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        await _eventRepository.UpdateAsync(eventEntity, ct);
        await _eventRepository.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> CanUserModifyEventAsync(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(eventId, ct);
        return eventEntity != null && eventEntity.OrganizerId == userId;
    }
}
