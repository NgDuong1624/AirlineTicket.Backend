using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Configurations;

public class AircraftModelConfiguration : IEntityTypeConfiguration<AircraftModel>
{
    public void Configure(EntityTypeBuilder<AircraftModel> builder)
    {
        builder.HasKey(x => x.Id);
    }
}