using AirlineTicket.Modules.Logs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Logs.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the <see cref="SystemLog"/> entity.
/// </summary>
public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SystemLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>()
            .HasMaxLength(50);

        builder.Property(x => x.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Level)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Message)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasMaxLength(200);
            
        builder.Property(x => x.IpAddress)
            .HasMaxLength(50);

        builder.Property(x => x.IsSystemLog)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.AirlineId, x.CreatedAt });
        builder.HasIndex(x => x.Type);
    }
}
