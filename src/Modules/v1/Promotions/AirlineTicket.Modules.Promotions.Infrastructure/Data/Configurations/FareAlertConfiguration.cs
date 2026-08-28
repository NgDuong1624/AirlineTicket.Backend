using AirlineTicket.Modules.Promotions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Promotions.Infrastructure.Data.Configurations;

public class FareAlertConfiguration : IEntityTypeConfiguration<FareAlert>
{
    public void Configure(EntityTypeBuilder<FareAlert> builder)
    {
        builder.ToTable("fare_alerts", "promotions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TargetPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CurrentLowestPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.LastNotifiedPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .HasDefaultValue("VND")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.OriginAirportId, x.DestinationAirportId, x.DepartureDate })
            .IsUnique()
            .HasDatabaseName("uq_fare_alerts_user_route");

        builder.HasIndex(x => new { x.OriginAirportId, x.DestinationAirportId, x.DepartureDate, x.IsActive })
            .HasDatabaseName("idx_fare_alerts_active_route");

        builder.HasIndex(x => new { x.UserId, x.IsActive })
            .HasDatabaseName("idx_fare_alerts_user_active");
    }
}