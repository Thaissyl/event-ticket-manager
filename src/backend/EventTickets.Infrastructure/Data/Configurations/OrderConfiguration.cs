using EventTickets.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTickets.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.GuestEmail)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.GuestName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.PaymentCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(o => o.PaymentCode)
            .IsUnique();

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
