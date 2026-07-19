using AirlineTicket.BuildingBlocks.Logging;
using AirlineTicket.Modules.Logs.Domain.Entities;
using AirlineTicket.Modules.Logs.Infrastructure.Data;
using BuildingBlocksLogType = AirlineTicket.BuildingBlocks.Domain.Enums.LogType;

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
        string? ipAddress = null,
        BuildingBlocksLogType? type = null,
        bool isSystemLog = false,
        string? metadata = null)
    {
        var log = new SystemLog
        {
            Id = Guid.NewGuid(),
            Type = (BuildingBlocksLogType)(type ?? BuildingBlocksLogType.Create),
            Metadata = metadata,
            Level = level,
            Message = message,
            Source = source,
            Exception = exception,
            UserId = userId,
            AirlineId = airlineId,
            IpAddress = ipAddress,
            IsSystemLog = isSystemLog,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.SystemLogs.Add(log);
        await _dbContext.SaveChangesAsync();
    }
}
