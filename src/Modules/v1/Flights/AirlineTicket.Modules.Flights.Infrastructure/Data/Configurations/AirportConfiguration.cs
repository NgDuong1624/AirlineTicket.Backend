using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Configurations;

public class AirportConfiguration : IEntityTypeConfiguration<Airport>
{
    public void Configure(EntityTypeBuilder<Airport> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IataCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(x => x.IataCode)
            .IsUnique();

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NameVi)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CityEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CityVi)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CountryCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Timezone)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Latitude)
            .HasPrecision(18, 6);

        builder.Property(x => x.Longitude)
            .HasPrecision(18, 6);
    }
}