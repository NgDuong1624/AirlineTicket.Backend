using AirlineTicket.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Users.Infrastructure.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Schema riêng cho Users
        modelBuilder.HasDefaultSchema("users");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
    }
}
