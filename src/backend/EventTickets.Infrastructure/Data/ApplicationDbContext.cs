using EventTickets.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<TicketTier> TicketTiers => Set<TicketTier>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<CartReservation> CartReservations => Set<CartReservation>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<SePayWebhook> SePayWebhooks => Set<SePayWebhook>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply entity configurations from separate configuration classes
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure indexes
        builder.Entity<Event>()
            .HasIndex(e => e.OrganizerId);

        builder.Entity<Event>()
            .HasIndex(e => e.Status);

        builder.Entity<Ticket>()
            .HasIndex(t => t.QrCode)
            .IsUnique();

        builder.Entity<CartReservation>()
            .HasIndex(cr => cr.SessionId);

        builder.Entity<CartReservation>()
            .HasIndex(cr => cr.ExpiresAt);

        builder.Entity<CartReservation>()
            .HasIndex(cr => new { cr.TicketTierId, cr.ExpiresAt });

        builder.Entity<PromoCode>()
            .HasIndex(p => p.Code)
            .IsUnique();

        builder.Entity<SePayWebhook>()
            .HasIndex(w => w.SePayTransactionId)
            .IsUnique();

        builder.Entity<Payment>()
            .HasIndex(p => p.SePayTransactionId)
            .IsUnique();

        builder.Entity<Payment>()
            .HasIndex(p => p.OrderId)
            .IsUnique();
    }
}
