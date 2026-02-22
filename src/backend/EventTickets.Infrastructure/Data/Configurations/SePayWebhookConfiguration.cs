using EventTickets.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTickets.Infrastructure.Data.Configurations;

public class SePayWebhookConfiguration : IEntityTypeConfiguration<SePayWebhook>
{
    public void Configure(EntityTypeBuilder<SePayWebhook> builder)
    {
        builder.Property(w => w.SePayTransactionId)
            .IsRequired();

        builder.Property(w => w.Payload)
            .IsRequired();

        builder.Property(w => w.Processed)
            .IsRequired();

        builder.Property(w => w.ProcessingError)
            .HasMaxLength(500);

        builder.Property(w => w.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
    }
}
