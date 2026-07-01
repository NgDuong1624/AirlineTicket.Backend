using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;

namespace AirlineTicket.Modules.Bookings.Application.Features.Revenue.Queries;

public record GetRevenueTrendsQuery(Guid AirlineId, DateTime FromDate, DateTime ToDate) : IQuery<Result<List<DailyRevenue>>>;

public class GetRevenueTrendsQueryHandler : IQueryHandler<GetRevenueTrendsQuery, Result<List<DailyRevenue>>>
{
    private readonly IRevenueRepository _revenueRepository;

    public GetRevenueTrendsQueryHandler(IRevenueRepository revenueRepository)
    {
        _revenueRepository = revenueRepository;
    }

    public async Task<Result<List<DailyRevenue>>> Handle(GetRevenueTrendsQuery request, CancellationToken cancellationToken)
    {
        var trends = await _revenueRepository.GetRevenueTrendsAsync(request.AirlineId, request.FromDate, request.ToDate, cancellationToken);
        return Result.Success(trends);
    }
}
