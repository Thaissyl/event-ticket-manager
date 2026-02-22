using EventTickets.Core.Entities;
using EventTickets.Core.Enums;

namespace EventTickets.Core.DTOs;

public record EventResponse(
    Guid Id,
    string Title,
    string? Description,
    string VenueName,
    string VenueAddress,
    string VenueCity,
    DateTime StartDateTime,
    DateTime EndDateTime,
    EventStatus Status,
    string? ImageUrl,
    int TotalCapacity,
    DateTime CreatedAt
);

public record CreateEventRequest(
    string Title,
    string? Description,
    string VenueName,
    string VenueAddress,
    string VenueCity,
    DateTime StartDateTime,
    DateTime EndDateTime,
    string? ImageUrl,
    int TotalCapacity
);

public record UpdateEventRequest(
    string Title,
    string? Description,
    string VenueName,
    string VenueAddress,
    string VenueCity,
    DateTime StartDateTime,
    DateTime EndDateTime,
    string? ImageUrl,
    int TotalCapacity,
    EventStatus Status
);

public record EventListResponse(
    IEnumerable<EventResponse> Events,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
