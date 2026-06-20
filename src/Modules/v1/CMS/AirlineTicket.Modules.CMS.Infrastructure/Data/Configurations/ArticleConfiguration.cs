using AirlineTicket.Modules.CMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.CMS.Infrastructure.Data.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(300);
            
        builder.Property(x => x.Summary)
            .HasMaxLength(1000);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(x => x.AuthorId)
            .IsRequired();
            
        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasOne(x => x.Category)
            .WithMany(c => c.Articles)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}