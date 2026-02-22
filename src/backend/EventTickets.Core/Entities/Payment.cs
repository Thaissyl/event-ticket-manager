using EventTickets.Core.Enums;

namespace EventTickets.Core.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public long? SePayTransactionId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
}
