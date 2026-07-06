using AirlineTicket.Modules.Logs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Logs.Infrastructure.Data;

/// <summary>
/// Database context for managing system logs.
/// </summary>
public class LogsDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogsDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public LogsDbContext(DbContextOptions<LogsDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the database set for system logs.
    /// </summary>
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("dbo");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LogsDbContext).Assembly);
    }
}
