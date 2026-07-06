using AirlineTicket.BuildingBlocks.Logging;
using AirlineTicket.Modules.Logs.Domain.Entities;
using AirlineTicket.Modules.Logs.Infrastructure.Data;

namespace AirlineTicket.Modules.Logs.Infrastructure.Services;

/// <summary>
/// Service implementation for writing system logs to the database.
/// </summary>
public class SystemLogService : ISystemLogService
{
    private readonly LogsDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemLogService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context for logs.</param>
    public SystemLogService(LogsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task LogAsync(
        string level,
        string message,
        string? source = null,
        string? exception = null,
        Guid? userId = null,
        Guid? airlineId = null,
        string? ipAddress = null)
    {
        var log = new SystemLog
        {
            Id = Guid.NewGuid(),
            Level = level,
            Message = message,
            Source = source,
            Exception = exception,
            UserId = userId,
            AirlineId = airlineId,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.SystemLogs.Add(log);
        await _dbContext.SaveChangesAsync();
    }
}
