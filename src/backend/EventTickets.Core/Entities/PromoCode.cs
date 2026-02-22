using EventTickets.Core.Enums;

namespace EventTickets.Core.Entities;

public class PromoCode
{
    public Guid Id { get; set; }
    public Guid? EventId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public int MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }

    // Navigation properties
    public virtual Event? Event { get; set; }
}
