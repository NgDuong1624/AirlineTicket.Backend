using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Configurations;

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.BasePrice)
            .HasPrecision(18, 2);

        // Ngăn chặn lỗi multiple cascade paths của SQL Server
        builder.HasOne(x => x.Route)
            .WithMany()
            .HasForeignKey(x => x.RouteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Airplane)
            .WithMany()
            .HasForeignKey(x => x.AirplaneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
