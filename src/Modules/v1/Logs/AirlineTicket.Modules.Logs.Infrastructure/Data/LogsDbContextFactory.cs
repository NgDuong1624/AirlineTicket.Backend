using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AirlineTicket.Modules.Logs.Infrastructure.Data;

public class LogsDbContextFactory : IDesignTimeDbContextFactory<LogsDbContext>
{
    public LogsDbContext CreateDbContext(string[] args)
    {
        var connectionString = System.Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Please set the 'ConnectionStrings__DefaultConnection' environment variable.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<LogsDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new LogsDbContext(optionsBuilder.Options);
    }
}
