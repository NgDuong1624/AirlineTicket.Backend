using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AirlineTicket.Modules.Interactions.Infrastructure.Data;

public class InteractionDbContextFactory : IDesignTimeDbContextFactory<InteractionDbContext>
{
    public InteractionDbContext CreateDbContext(string[] args)
    {
        var connectionString = System.Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Please set the 'ConnectionStrings__DefaultConnection' environment variable.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<InteractionDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new InteractionDbContext(optionsBuilder.Options);
    }
}
