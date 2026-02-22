using EventTickets.Core.Enums;

namespace EventTickets.Core.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid TicketTierId { get; set; }
    public string QrCode { get; set; } = string.Empty;
    public string QrCodeSignature { get; set; } = string.Empty;
    public string AttendeeName { get; set; } = string.Empty;
    public string AttendeeEmail { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual TicketTier TicketTier { get; set; } = null!;
}
