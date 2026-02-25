using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Enums;
using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventTickets.Infrastructure.Services;

public class CheckinService : ICheckinService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IQrCodeService _qrCodeService;
    private readonly ILogger<CheckinService> _logger;

    public CheckinService(
        ITicketRepository ticketRepository,
        IQrCodeService qrCodeService,
        ILogger<CheckinService> logger)
    {
        _ticketRepository = ticketRepository;
        _qrCodeService = qrCodeService;
        _logger = logger;
    }

    public async Task<CheckinResult> CheckInAsync(string qrCode, CancellationToken cancellationToken = default)
    {
        // Validate QR code format and signature
        if (!_qrCodeService.ValidateQrCode(qrCode, out var ticketId))
        {
            _logger.LogWarning("Invalid QR code format: {QrCode}", qrCode);
            return new CheckinResult(false, "Invalid QR code", null, null, null, null);
        }

        // Find ticket with related data
        var ticket = await _ticketRepository.GetWithTicketTierAndOrderAsync(ticketId, cancellationToken);
        if (ticket == null)
        {
            _logger.LogWarning("Ticket not found: {TicketId}", ticketId);
            return new CheckinResult(false, "Ticket not found", null, null, null, null);
        }

        // Check ticket status
        if (ticket.Status != TicketStatus.Valid)
        {
            var statusMessage = ticket.Status switch
            {
                TicketStatus.Cancelled => "Ticket has been cancelled",
                TicketStatus.Refunded => "Ticket has been refunded",
                TicketStatus.Used => "Ticket already checked in",
                _ => $"Invalid ticket status: {ticket.Status}"
            };
            return new CheckinResult(false, statusMessage, ticket.AttendeeName, ticket.TicketTier?.Name, null, ticket.CheckedInAt);
        }

        // Update check-in status
        ticket.Status = TicketStatus.Used;
        ticket.CheckedInAt = DateTime.UtcNow;

        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket checked in: {TicketId} for {AttendeeName}", ticketId, ticket.AttendeeName);

        // Get event name from order
        var eventName = ticket.Order?.Tickets.FirstOrDefault()?.TicketTier?.Event?.Title ?? "Unknown Event";

        return new CheckinResult(
            true,
            "Check-in successful",
            ticket.AttendeeName,
            ticket.TicketTier?.Name,
            eventName,
            ticket.CheckedInAt
        );
    }

    public async Task<TicketValidationResult> ValidateAsync(string qrCode, CancellationToken cancellationToken = default)
    {
        if (!_qrCodeService.ValidateQrCode(qrCode, out var ticketId))
        {
            return new TicketValidationResult(false, "Invalid QR code", null, null, TicketStatus.Cancelled);
        }

        var ticket = await _ticketRepository.GetWithTicketTierAsync(ticketId, cancellationToken);
        if (ticket == null)
        {
            return new TicketValidationResult(false, "Ticket not found", null, null, TicketStatus.Cancelled);
        }

        var isValid = ticket.Status == TicketStatus.Valid;
        var message = ticket.Status switch
        {
            TicketStatus.Valid => "Ticket is valid",
            TicketStatus.Used => "Ticket already checked in",
            TicketStatus.Cancelled => "Ticket has been cancelled",
            TicketStatus.Refunded => "Ticket has been refunded",
            _ => "Invalid ticket status"
        };

        return new TicketValidationResult(isValid, message, ticket.AttendeeName, ticket.TicketTier?.Name, ticket.Status);
    }

    public async Task<bool> UndoCheckInAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null)
            return false;

        if (ticket.Status != TicketStatus.Used)
            return false;

        ticket.Status = TicketStatus.Valid;
        ticket.CheckedInAt = null;

        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Check-in undone for ticket: {TicketId}", ticketId);
        return true;
    }

    public async Task<CheckinStats> GetStatsAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var tickets = await _ticketRepository.GetByEventIdAsync(eventId, cancellationToken);

        var sold = tickets.Count();
        var checkedIn = tickets.Count(t => t.Status == TicketStatus.Used);
        var percentage = sold > 0 ? (checkedIn * 100.0 / sold) : 0;

        var byTier = tickets
            .GroupBy(t => t.TicketTier?.Name ?? "Unknown")
            .Select(g => new TierStats(
                g.Key,
                g.Count(),
                g.Count(t => t.Status == TicketStatus.Used)
            ))
            .ToArray();

        return new CheckinStats(sold, checkedIn, Math.Round(percentage, 1), byTier);
    }
}
