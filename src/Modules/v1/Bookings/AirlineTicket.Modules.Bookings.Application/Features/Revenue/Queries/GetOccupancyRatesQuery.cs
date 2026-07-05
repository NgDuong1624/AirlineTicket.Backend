using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;

namespace AirlineTicket.Modules.Bookings.Application.Features.Revenue.Queries;

public record GetOccupancyRatesQuery(Guid AirlineId, DateTime FromDate, DateTime ToDate) : IQuery<Result<List<DailyOccupancy>>>;

public class GetOccupancyRatesQueryHandler : IQueryHandler<GetOccupancyRatesQuery, Result<List<DailyOccupancy>>>
{
    private readonly IRevenueRepository _revenueRepository;

    public GetOccupancyRatesQueryHandler(IRevenueRepository revenueRepository)
    {
        _revenueRepository = revenueRepository;
    }

    public async Task<Result<List<DailyOccupancy>>> Handle(GetOccupancyRatesQuery request, CancellationToken cancellationToken)
    {
        var occupancy = await _revenueRepository.GetOccupancyRatesAsync(request.AirlineId, request.FromDate, request.ToDate, cancellationToken);
        return Result.Success(occupancy);
    }
}
