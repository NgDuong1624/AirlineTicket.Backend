using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Configurations;

public class AirplaneConfiguration : IEntityTypeConfiguration<Airplane>
{
    public void Configure(EntityTypeBuilder<Airplane> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Airline)
            .WithMany(a => a.Airplanes)
            .HasForeignKey(x => x.AirlineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AircraftModel)
            .WithMany(m => m.Airplanes)
            .HasForeignKey(x => x.AircraftModelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
