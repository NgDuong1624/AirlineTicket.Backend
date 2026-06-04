using AirlineTicket.Modules.Promotions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Promotions.Infrastructure.Data.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.DiscountValue)
            .HasPrecision(18, 2);
    }
}
