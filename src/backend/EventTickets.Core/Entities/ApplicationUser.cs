using Microsoft.AspNetCore.Identity;

namespace EventTickets.Core.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public Enums.UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
