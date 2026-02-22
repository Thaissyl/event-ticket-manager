using EventTickets.Core.Enums;

namespace EventTickets.Core.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string PaymentCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public uint RowVersion { get; set; }

    // Navigation properties
    public virtual ApplicationUser? User { get; set; }
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public virtual Payment? Payment { get; set; }
}
