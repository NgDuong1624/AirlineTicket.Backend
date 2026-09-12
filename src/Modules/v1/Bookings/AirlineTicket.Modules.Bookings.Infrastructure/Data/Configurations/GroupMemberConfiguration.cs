using AirlineTicket.Modules.Bookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Configurations;

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.ToTable("group_members", "bookings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PassengerName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.PassengerEmail)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.PassengerPhone)
            .HasMaxLength(25);

        builder.Property(x => x.SeatNumber)
            .HasMaxLength(10);

        builder.Property(x => x.ReturnSeatNumber)
            .HasMaxLength(10);

        builder.Property(x => x.AssignedAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.PaidAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.PaymentTransactionId)
            .HasMaxLength(100);

        builder.Property(x => x.PaymentProvider)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => x.GroupBookingId);
        builder.HasIndex(x => x.PassengerEmail);
    }
}
