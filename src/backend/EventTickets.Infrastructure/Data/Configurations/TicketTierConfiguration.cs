using EventTickets.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTickets.Infrastructure.Data.Configurations;

public class TicketTierConfiguration : IEntityTypeConfiguration<TicketTier>
{
    public void Configure(EntityTypeBuilder<TicketTier> builder)
    {
        builder.HasKey(tt => tt.Id);

        builder.Property(tt => tt.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(tt => tt.Description)
            .HasMaxLength(500);

        builder.Property(tt => tt.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(tt => tt.QuantityTotal)
            .IsRequired();

        builder.Property(tt => tt.QuantitySold)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(tt => tt.QuantityReserved)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(tt => tt.SaleStartDateTime)
            .IsRequired();

        builder.Property(tt => tt.SaleEndDateTime)
            .IsRequired();

        builder.Property(tt => tt.RowVersion)
            .IsRowVersion();

        builder.HasOne(tt => tt.Event)
            .WithMany(e => e.TicketTiers)
            .HasForeignKey(tt => tt.EventId)
            .HasPrincipalKey(e => e.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
