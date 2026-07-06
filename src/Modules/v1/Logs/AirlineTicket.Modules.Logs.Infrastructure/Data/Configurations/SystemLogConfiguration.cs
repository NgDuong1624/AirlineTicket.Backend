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

        builder.Property(x => x.Level)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Message)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasMaxLength(200);
            
        builder.Property(x => x.IpAddress)
            .HasMaxLength(50);

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.AirlineId, x.CreatedAt });
    }
}
