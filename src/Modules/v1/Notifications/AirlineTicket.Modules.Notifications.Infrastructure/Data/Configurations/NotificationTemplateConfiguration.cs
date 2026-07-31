using AirlineTicket.Modules.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Notifications.Infrastructure.Data.Configurations;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.BodyTemplate)
            .IsRequired();

        builder.Property(x => x.Language)
            .HasMaxLength(10);
            
        builder.HasIndex(x => new { x.Code, x.Language })
            .IsUnique();
    }
}
