using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetAdminFlightsQuery(int PageIndex = 1, int PageSize = 10) : IQuery<Result<PagedResult<FlightDto>>>;

internal sealed class GetAdminFlightsQueryHandler : IQueryHandler<GetAdminFlightsQuery, Result<PagedResult<FlightDto>>>
{
    private readonly IFlightRepository _flightRepository;

    public GetAdminFlightsQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<Result<PagedResult<FlightDto>>> Handle(GetAdminFlightsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _flightRepository.GetAllAsync(request.PageIndex, request.PageSize, cancellationToken);
        return Result.Success(PagedResult<FlightDto>.Success(items, request.PageIndex, request.PageSize, totalCount));
    }
}
