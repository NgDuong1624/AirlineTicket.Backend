using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Configurations;

public class AirplaneSeatConfiguration : IEntityTypeConfiguration<AirplaneSeat>
{
    public void Configure(EntityTypeBuilder<AirplaneSeat> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PriceMultiplier)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.Airplane)
            .WithMany(a => a.AirplaneSeats)
            .HasForeignKey(x => x.AirplaneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.AirplaneId);
    }
}
