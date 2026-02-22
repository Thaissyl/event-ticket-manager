using EventTickets.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTickets.Infrastructure.Data.Configurations;

public class CartReservationConfiguration : IEntityTypeConfiguration<CartReservation>
{
    public void Configure(EntityTypeBuilder<CartReservation> builder)
    {
        builder.Property(cr => cr.SessionId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(cr => cr.Quantity)
            .IsRequired();

        builder.Property(cr => cr.ExpiresAt)
            .IsRequired();

        builder.Property(cr => cr.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasOne(cr => cr.TicketTier)
            .WithMany(tt => tt.CartReservations)
            .HasForeignKey(cr => cr.TicketTierId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
