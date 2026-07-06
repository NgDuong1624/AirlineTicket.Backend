using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Logs.Application.Contracts;

/// <summary>
/// Defines repository operations for accessing system logs.
/// </summary>
public interface ILogRepository
{
    /// <summary>
    /// Retrieves a paged list of system logs based on filter criteria.
    /// </summary>
    /// <param name="pageIndex">The page index for pagination.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="airlineId">Optional airline ID to filter logs.</param>
    /// <param name="level">Optional log level to filter logs.</param>
    /// <param name="search">Optional search term to filter log messages, sources, exceptions, or IP addresses.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching log DTOs.</returns>
    Task<PagedResult<LogDto>> GetLogsAsync(
        int pageIndex,
        int pageSize,
        Guid? airlineId = null,
        string? level = null,
        string? search = null,
        CancellationToken cancellationToken = default);
}
