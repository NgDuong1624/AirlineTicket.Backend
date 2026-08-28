using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Configurations;

public class FlightPriceHistoryConfiguration : IEntityTypeConfiguration<FlightPriceHistory>
{
    public void Configure(EntityTypeBuilder<FlightPriceHistory> builder)
    {
        builder.ToTable("flight_price_histories", "flights");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.SeatClass)
            .HasMaxLength(20)
            .HasDefaultValue("Economy")
            .IsRequired();

        builder.HasOne(x => x.Flight)
            .WithMany()
            .HasForeignKey(x => x.FlightId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Route)
            .WithMany()
            .HasForeignKey(x => x.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.RouteId, x.RecordedAt })
            .HasDatabaseName("idx_flight_price_history_route_date");

        builder.HasIndex(x => new { x.FlightId, x.RecordedAt })
            .HasDatabaseName("idx_flight_price_history_flight_date");
    }
}