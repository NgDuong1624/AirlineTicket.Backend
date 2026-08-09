using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Logs.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Logs.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for accessing system logs from the database.
/// </summary>
public class LogRepository : ILogRepository
{
    private readonly LogsDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for logs.</param>
    public LogRepository(LogsDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<PagedResult<LogDto>> GetLogsAsync(
        int pageIndex,
        int pageSize,
        Guid? airlineId = null,
        string? level = null,
        string? search = null,
        bool? isSystemLog = null,
        DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        pageIndex = Math.Max(pageIndex, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.SystemLogs.AsNoTracking();

        if (airlineId.HasValue)
        {
            query = query.Where(x => x.AirlineId == airlineId.Value);
        }

        if (isSystemLog.HasValue)
        {
            query = query.Where(x => x.IsSystemLog == isSystemLog.Value);
        }

        if (!string.IsNullOrWhiteSpace(level))
        {
            query = query.Where(x => x.Level == level);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Message.Contains(search) 
                                  || (x.Source != null && x.Source.Contains(search))
                                  || (x.Exception != null && x.Exception.Contains(search))
                                  || (x.IpAddress != null && x.IpAddress.Contains(search)));
        }

        if (date.HasValue)
        {
            query = query.Where(x => x.CreatedAt.Date == date.Value.Date);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new LogDto
            {
                Id = x.Id,
                Type = x.Type.ToString(),
                Metadata = x.Metadata,
                Level = x.Level,
                Message = x.Message,
                Source = x.Source,
                Exception = x.Exception,
                UserId = x.UserId,
                AirlineId = x.AirlineId,
                IpAddress = x.IpAddress,
                CreatedAt = x.CreatedAt,
                IsSystemLog = x.IsSystemLog
            })
            .ToListAsync(cancellationToken);

        return PagedResult<LogDto>.Success(items, pageIndex, pageSize, totalCount);
    }

    public async Task<PagedResult<LogDto>> GetAirlineLogsAsync(
        int pageIndex,
        int pageSize,
        Guid? airlineId = null,
        string? level = null,
        string? search = null,
        DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        pageIndex = Math.Max(pageIndex, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.SystemLogs.AsNoTracking();

        if (airlineId.HasValue)
        {
            query = query.Where(x => x.AirlineId == airlineId.Value);
        }

        if (!string.IsNullOrWhiteSpace(level))
        {
            query = query.Where(x => x.Level == level);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Message.Contains(search) 
                                  || (x.Source != null && x.Source.Contains(search))
                                  || (x.Exception != null && x.Exception.Contains(search))
                                  || (x.IpAddress != null && x.IpAddress.Contains(search)));
        }

        if (date.HasValue)
        {
            query = query.Where(x => x.CreatedAt.Date == date.Value.Date);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new LogDto
            {
                Id = x.Id,
                Type = x.Type.ToString(),
                Metadata = x.Metadata,
                Level = x.Level,
                Message = x.Message,
                Source = x.Source,
                Exception = x.Exception,
                UserId = x.UserId,
                AirlineId = x.AirlineId,
                IpAddress = x.IpAddress,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return PagedResult<LogDto>.Success(items, pageIndex, pageSize, totalCount);
    }
}
