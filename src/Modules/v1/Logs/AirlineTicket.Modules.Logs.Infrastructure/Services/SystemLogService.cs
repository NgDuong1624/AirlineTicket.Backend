using AirlineTicket.BuildingBlocks.Logging;
using AirlineTicket.Modules.Logs.Domain.Entities;
using AirlineTicket.Modules.Logs.Infrastructure.Data;

namespace AirlineTicket.Modules.Logs.Infrastructure.Services;

public class SystemLogService : ISystemLogService
{
    private readonly LogsDbContext _dbContext;

    public SystemLogService(LogsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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
