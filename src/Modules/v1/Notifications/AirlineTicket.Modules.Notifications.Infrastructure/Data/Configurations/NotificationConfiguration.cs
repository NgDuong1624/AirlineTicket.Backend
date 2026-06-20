using AirlineTicket.Modules.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Notifications.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Recipient)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Subject)
            .HasMaxLength(300);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(1000);
    }
}
