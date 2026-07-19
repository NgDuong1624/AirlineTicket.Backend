using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Logs.Application.Contracts;

namespace AirlineTicket.Modules.Logs.Application.Features;

/// <summary>
/// Represents a query to retrieve logs specific to a partner airline.
/// </summary>
/// <param name="AirlineId">The unique identifier of the partner airline.</param>
/// <param name="PageIndex">The page index for pagination, defaults to 1.</param>
/// <param name="PageSize">The number of items per page, defaults to 10.</param>
/// <param name="Level">Optional log level to filter by (e.g., "Error", "Warning").</param>
/// <param name="Search">Optional search string to filter log messages.</param>
public record GetPartnerLogsQuery(
    Guid AirlineId, 
    int PageIndex = 1, 
    int PageSize = 10,
    string? Level = null,
    string? Search = null) : IQuery<PagedResult<LogDto>>;

/// <summary>
/// Handles the <see cref="GetPartnerLogsQuery"/> to retrieve partner logs.
/// </summary>
public class GetPartnerLogsQueryHandler : IQueryHandler<GetPartnerLogsQuery, PagedResult<LogDto>>
{
    private readonly ILogRepository _logRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPartnerLogsQueryHandler"/> class.
    /// </summary>
    /// <param name="logRepository">The log repository.</param>
    public GetPartnerLogsQueryHandler(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    /// <summary>
    /// Handles the partner log query asynchronously.
    /// </summary>
    /// <param name="request">The <see cref="GetPartnerLogsQuery"/> request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="PagedResult{LogDto}"/> containing the partner logs.</returns>
    public async Task<PagedResult<LogDto>> Handle(GetPartnerLogsQuery request, CancellationToken cancellationToken)
    {
        return await _logRepository.GetAirlineLogsAsync(
            request.PageIndex, 
            request.PageSize, 
            request.AirlineId, 
            request.Level, 
            request.Search, 
            cancellationToken);
    }
}
