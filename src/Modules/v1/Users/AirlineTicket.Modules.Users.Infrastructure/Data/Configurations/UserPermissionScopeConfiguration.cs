using AirlineTicket.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicket.Modules.Users.Infrastructure.Data.Configurations;

public class UserPermissionScopeConfiguration : IEntityTypeConfiguration<UserPermissionScope>
{
    public void Configure(EntityTypeBuilder<UserPermissionScope> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany(u => u.UserPermissionScopes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict để tránh nhiều đường cascade (multiple cascade paths) trên SQL Server
        builder.HasOne(x => x.Permission)
            .WithMany(p => p.UserPermissionScopes)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.AirportCode)
            .HasMaxLength(10);

        builder.Property(x => x.ScopeDescription)
            .HasMaxLength(250);
    }
}
