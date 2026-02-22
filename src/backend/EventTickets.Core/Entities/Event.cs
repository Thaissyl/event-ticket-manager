using EventTickets.Core.Enums;

namespace EventTickets.Core.Entities;

public class Event
{
    public Guid Id { get; set; }
    public Guid OrganizerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public string VenueAddress { get; set; } = string.Empty;
    public string VenueCity { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public EventStatus Status { get; set; }
    public string? ImageUrl { get; set; }
    public int TotalCapacity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public uint RowVersion { get; set; }

    // Navigation properties
    public virtual ApplicationUser Organizer { get; set; } = null!;
    public virtual ICollection<TicketTier> TicketTiers { get; set; } = new List<TicketTier>();
}
