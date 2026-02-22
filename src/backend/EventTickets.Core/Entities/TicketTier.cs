namespace EventTickets.Core.Entities;

public class TicketTier
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int QuantityTotal { get; set; }
    public int QuantitySold { get; set; }
    public int QuantityReserved { get; set; }
    public DateTime SaleStartDateTime { get; set; }
    public DateTime SaleEndDateTime { get; set; }
    public uint RowVersion { get; set; }

    // Navigation properties
    public virtual Event Event { get; set; } = null!;
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public virtual ICollection<CartReservation> CartReservations { get; set; } = new List<CartReservation>();
}
