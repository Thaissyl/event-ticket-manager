using EventTickets.Core.DTOs;
using EventTickets.Core.Enums;

namespace EventTickets.Core.Services;

public interface ICheckinService
{
    /// <summary>
    /// Checks in a ticket by QR code
    /// </summary>
    /// <param name="qrCode">QR code string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Check-in result with attendee info</returns>
    Task<CheckinResult> CheckInAsync(string qrCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a ticket without checking in
    /// </summary>
    /// <param name="qrCode">QR code string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Ticket validation result</returns>
    Task<TicketValidationResult> ValidateAsync(string qrCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Undoes a check-in (admin only)
    /// </summary>
    /// <param name="ticketId">Ticket ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> UndoCheckInAsync(Guid ticketId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets check-in statistics for an event
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Check-in statistics</returns>
    Task<CheckinStats> GetStatsAsync(Guid eventId, CancellationToken cancellationToken = default);
}

public record CheckinResult(
    bool Success,
    string Message,
    string? AttendeeName,
    string? TicketTierName,
    string? EventName,
    DateTime? CheckedInAt
);

public record TicketValidationResult(
    bool Valid,
    string Message,
    string? AttendeeName,
    string? TicketTierName,
    TicketStatus Status
);

public record CheckinStats(
    int TotalSold,
    int Used,
    double Percentage,
    TierStats[] ByTier
);

public record TierStats(
    string Name,
    int Sold,
    int Used
);
