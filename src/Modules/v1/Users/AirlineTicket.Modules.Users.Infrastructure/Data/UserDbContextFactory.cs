using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AirlineTicket.Modules.Users.Infrastructure.Data;

/// <summary>
/// Factory used for design-time (dotnet ef migrations/database update).
/// Allows EF Tools to create DbContext without building the API host project.
/// Retrieves the connection string from the ConnectionStrings__DefaultConnection environment variable.
/// </summary>
public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var connectionString = System.Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Please set the 'ConnectionStrings__DefaultConnection' environment variable " +
                "or configure it in appsettings.json.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new UserDbContext(optionsBuilder.Options);
    }
}
