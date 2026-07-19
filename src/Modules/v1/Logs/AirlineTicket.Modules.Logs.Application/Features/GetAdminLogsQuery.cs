using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Logs.Application.Contracts;

namespace AirlineTicket.Modules.Logs.Application.Features;

/// <summary>
/// Represents a query to retrieve administrative logs.
/// </summary>
/// <param name="PageIndex">The page index for pagination, defaults to 1.</param>
/// <param name="PageSize">The number of items per page, defaults to 10.</param>
/// <param name="Level">Optional log level to filter by (e.g., "Error", "Warning").</param>
/// <param name="Search">Optional search string to filter log messages.</param>
/// <param name="AirlineId">Optional airline ID to filter logs specific to an airline.</param>
/// <param name="IsSystemLog">Optional flag to filter for system logs.</param>
public record GetAdminLogsQuery(
    int PageIndex = 1, 
    int PageSize = 10,
    string? Level = null,
    string? Search = null,
    Guid? AirlineId = null,
    bool? IsSystemLog = null) : IQuery<PagedResult<LogDto>>;

/// <summary>
/// Handles the <see cref="GetAdminLogsQuery"/> to retrieve administrative logs.
/// </summary>
public class GetAdminLogsQueryHandler : IQueryHandler<GetAdminLogsQuery, PagedResult<LogDto>>
{
    private readonly ILogRepository _logRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAdminLogsQueryHandler"/> class.
    /// </summary>
    /// <param name="logRepository">The log repository.</param>
    public GetAdminLogsQueryHandler(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    /// <summary>
    /// Handles the log query asynchronously.
    /// </summary>
    /// <param name="request">The <see cref="GetAdminLogsQuery"/> request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="PagedResult{LogDto}"/> containing the administrative logs.</returns>
    public async Task<PagedResult<LogDto>> Handle(GetAdminLogsQuery request, CancellationToken cancellationToken)
    {
        return await _logRepository.GetLogsAsync(
            request.PageIndex, 
            request.PageSize, 
            request.AirlineId, 
            request.Level, 
            request.Search, 
            request.IsSystemLog, 
            cancellationToken);
    }
}
