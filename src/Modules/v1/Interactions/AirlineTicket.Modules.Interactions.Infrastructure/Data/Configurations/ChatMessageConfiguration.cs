using AirlineTicket.Modules.Interactions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Interactions.Infrastructure.Data.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();
        builder.Property(c => c.AirlineId).IsRequired();
        builder.Property(c => c.SenderRole).IsRequired().HasMaxLength(20);
        builder.Property(c => c.SenderName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.CustomerConnectionId).HasMaxLength(100);
        builder.Property(c => c.StaffConnectionId).HasMaxLength(100);
        builder.Property(c => c.Content).IsRequired();
        builder.Property(c => c.SentAt).IsRequired();
        builder.HasIndex(c => new { c.AirlineId, c.SentAt });
    }
}