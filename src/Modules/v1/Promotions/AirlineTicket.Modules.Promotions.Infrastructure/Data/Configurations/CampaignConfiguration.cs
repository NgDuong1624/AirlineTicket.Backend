using AirlineTicket.Modules.Promotions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Promotions.Infrastructure.Data.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TitleEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TitleVi)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.BannerUrl)
            .HasMaxLength(500);

        builder.Property(x => x.ContentEn)
            .HasMaxLength(2000);

        builder.Property(x => x.ContentVi)
            .HasMaxLength(2000);
    }
}