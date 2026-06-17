using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.HasKey(x => x.Id);

        // Ngăn chặn lỗi multiple cascade paths của SQL Server
        builder.HasOne(x => x.OriginAirport)
            .WithMany()
            .HasForeignKey(x => x.OriginAirportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DestinationAirport)
            .WithMany()
            .HasForeignKey(x => x.DestinationAirportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Airline)
            .WithMany(a => a.Routes)
            .HasForeignKey(x => x.AirlineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
