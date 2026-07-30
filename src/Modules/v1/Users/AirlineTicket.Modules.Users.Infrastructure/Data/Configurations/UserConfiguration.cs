using AirlineTicket.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Users.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.GoogleId)
            .HasMaxLength(100);

        builder.HasIndex(x => x.GoogleId)
            .IsUnique()
            .HasFilter("\"GoogleId\" IS NOT NULL");

        builder.Property(x => x.AuthProvider)
            .HasMaxLength(50);

        builder.HasOne(x => x.RoleEntity)
            .WithMany()
            .HasForeignKey(x => x.Role)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
