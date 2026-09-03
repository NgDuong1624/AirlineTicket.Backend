using AirlineTicket.Modules.Bookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Configurations;

public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        builder.ToTable("webhook_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ProviderEventId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Error)
            .HasMaxLength(1024);

        builder.Property(x => x.ReceivedAt)
            .IsRequired();

        // Unique index for idempotent fast-ack and duplicate webhook defense
        builder.HasIndex(x => new { x.Provider, x.ProviderEventId })
            .IsUnique();

        builder.HasIndex(x => new { x.Status, x.ReceivedAt });
    }
}
