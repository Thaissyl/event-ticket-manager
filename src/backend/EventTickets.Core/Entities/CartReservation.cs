namespace EventTickets.Core.Entities;

public class CartReservation
{
    public Guid Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public Guid TicketTierId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual TicketTier TicketTier { get; set; } = null!;
}
