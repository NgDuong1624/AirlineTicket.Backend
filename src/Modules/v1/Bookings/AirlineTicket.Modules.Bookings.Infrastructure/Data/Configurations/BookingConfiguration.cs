using AirlineTicket.Modules.Bookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.TotalPrice)
            .HasPrecision(18, 2);

        builder.HasIndex(x => new { x.CreatedAt, x.Status });
    }
}
