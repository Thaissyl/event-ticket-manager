using EventTickets.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTickets.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.QrCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.QrCodeSignature)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.AttendeeName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.AttendeeEmail)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasOne(t => t.Order)
            .WithMany(o => o.Tickets)
            .HasForeignKey(t => t.OrderId)
            .HasPrincipalKey(o => o.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.TicketTier)
            .WithMany(tt => tt.Tickets)
            .HasForeignKey(t => t.TicketTierId)
            .HasPrincipalKey(tt => tt.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
