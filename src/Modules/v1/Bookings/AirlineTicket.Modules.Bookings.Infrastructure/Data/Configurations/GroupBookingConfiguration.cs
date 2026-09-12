using AirlineTicket.Modules.Bookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Configurations;

public class GroupBookingConfiguration : IEntityTypeConfiguration<GroupBooking>
{
    public void Configure(EntityTypeBuilder<GroupBooking> builder)
    {
        builder.ToTable("group_bookings", "bookings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.GroupName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.InviteCode)
            .HasMaxLength(12)
            .IsRequired();

        builder.HasIndex(x => x.InviteCode)
            .IsUnique();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.PaidAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.SplitStrategy)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => new { x.Status, x.ExpiresAt });

        builder.HasMany(x => x.Members)
            .WithOne(x => x.GroupBooking)
            .HasForeignKey(x => x.GroupBookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
