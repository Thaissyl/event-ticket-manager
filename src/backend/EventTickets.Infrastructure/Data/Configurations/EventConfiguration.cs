using EventTickets.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTickets.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .IsRequired();

        builder.Property(e => e.VenueName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.VenueAddress)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.VenueCity)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.TotalCapacity)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.Property(e => e.StartDateTime)
            .IsRequired();

        builder.Property(e => e.EndDateTime)
            .IsRequired();

        builder.HasOne(e => e.Organizer)
            .WithMany(u => u.OrganizedEvents)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
