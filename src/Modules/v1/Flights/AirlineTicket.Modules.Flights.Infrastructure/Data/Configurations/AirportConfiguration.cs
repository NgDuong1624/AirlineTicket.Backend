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
            
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Country)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Timezone)
            .IsRequired()
            .HasMaxLength(50);
    }
}
