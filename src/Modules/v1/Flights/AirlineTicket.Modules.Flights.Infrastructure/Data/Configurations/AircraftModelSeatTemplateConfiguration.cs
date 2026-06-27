using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Configurations;

public class AircraftModelSeatTemplateConfiguration : IEntityTypeConfiguration<AircraftModelSeatTemplate>
{
    public void Configure(EntityTypeBuilder<AircraftModelSeatTemplate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PriceMultiplier)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.AircraftModel)
            .WithMany(m => m.SeatTemplates)
            .HasForeignKey(x => x.AircraftModelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}