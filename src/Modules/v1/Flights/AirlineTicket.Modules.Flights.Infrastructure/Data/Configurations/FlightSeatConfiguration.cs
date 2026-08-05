using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Configurations;

public class FlightSeatConfiguration : IEntityTypeConfiguration<FlightSeat>
{
    public void Configure(EntityTypeBuilder<FlightSeat> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PriceOverride)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.Flight)
            .WithMany(f => f.FlightSeats)
            .HasForeignKey(x => x.FlightId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FlightId, x.IsAvailable });
    }
}