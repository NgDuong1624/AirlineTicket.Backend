using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DistanceKm)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.OriginAirport)
            .WithMany(a => a.OriginRoutes)
            .HasForeignKey(x => x.OriginAirportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DestinationAirport)
            .WithMany(a => a.DestinationRoutes)
            .HasForeignKey(x => x.DestinationAirportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Airline)
            .WithMany(a => a.Routes)
            .HasForeignKey(x => x.AirlineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
