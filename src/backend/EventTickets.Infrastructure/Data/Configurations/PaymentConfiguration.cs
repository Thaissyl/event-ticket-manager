using EventTickets.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTickets.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.ReferenceCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasOne(p => p.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<Order>(o => o.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
