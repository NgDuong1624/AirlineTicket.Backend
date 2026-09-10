using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Configurations;

public class FlightTelemetryConfiguration : IEntityTypeConfiguration<FlightTelemetry>
{
    public void Configure(EntityTypeBuilder<FlightTelemetry> builder)
    {
        builder.ToTable("flight_telemetry");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Latitude)
            .IsRequired();

        builder.Property(x => x.Longitude)
            .IsRequired();

        builder.Property(x => x.AltitudeFeet)
            .HasDefaultValue(0);

        builder.Property(x => x.SpeedKnots)
            .HasDefaultValue(0);

        builder.Property(x => x.HeadingDegrees)
            .HasDefaultValue(0);

        builder.Property(x => x.ProgressPercentage)
            .HasDefaultValue((short)0);

        builder.Property(x => x.EstimatedArrival)
            .IsRequired();

        builder.Property(x => x.RecordedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(x => x.Flight)
            .WithMany(f => f.Telemetries)
            .HasForeignKey(x => x.FlightId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.FlightId, x.RecordedAt });
    }
}
