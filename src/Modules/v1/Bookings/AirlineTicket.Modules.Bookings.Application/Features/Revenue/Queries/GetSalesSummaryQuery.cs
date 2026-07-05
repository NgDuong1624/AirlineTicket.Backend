using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;

namespace AirlineTicket.Modules.Bookings.Application.Features.Revenue.Queries;

public record GetSalesSummaryQuery(Guid AirlineId, DateTime FromDate, DateTime ToDate) : IQuery<Result<DailySalesSummary>>;

public class GetSalesSummaryQueryHandler : IQueryHandler<GetSalesSummaryQuery, Result<DailySalesSummary>>
{
    private readonly IRevenueRepository _revenueRepository;

    public GetSalesSummaryQueryHandler(IRevenueRepository revenueRepository)
    {
        _revenueRepository = revenueRepository;
    }

    public async Task<Result<DailySalesSummary>> Handle(GetSalesSummaryQuery request, CancellationToken cancellationToken)
    {
        var summary = await _revenueRepository.GetSalesSummaryAsync(request.AirlineId, request.FromDate, request.ToDate, cancellationToken);
        return Result.Success(summary ?? new DailySalesSummary(0, 0, 0));
    }
}
