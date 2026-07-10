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

        builder.HasOne(x => x.RoleEntity)
            .WithMany()
            .HasForeignKey(x => x.Role)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
