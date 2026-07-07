using AirlineTicket.Modules.Promotions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Promotions.Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(x => x.Code)
            .IsUnique();
            
        builder.Property(x => x.DiscountValue)
            .HasPrecision(18, 2);
            
        builder.Property(x => x.MinOrderValue)
            .HasPrecision(18, 2);
            
        builder.Property(x => x.MaxDiscountAmount)
            .HasPrecision(18, 2);
    }
}
