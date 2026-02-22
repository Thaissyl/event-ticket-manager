using EventTickets.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTickets.Infrastructure.Data.Configurations;

public class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.Property(p => p.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.DiscountType)
            .IsRequired();

        builder.Property(p => p.DiscountValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.MaxUses)
            .IsRequired();

        builder.Property(p => p.CurrentUses)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(p => p.ValidFrom)
            .IsRequired();

        builder.Property(p => p.ValidUntil)
            .IsRequired();

        builder.HasOne(p => p.Event)
            .WithMany()
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
