using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetStaffFlightsQuery(string? Search = null) : IQuery<Result<List<StaffFlightListItemDto>>>;

public class GetStaffFlightsQueryHandler : IQueryHandler<GetStaffFlightsQuery, Result<List<StaffFlightListItemDto>>>
{
    private readonly IFlightRepository _flightRepository;

    public GetStaffFlightsQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<Result<List<StaffFlightListItemDto>>> Handle(GetStaffFlightsQuery request, CancellationToken cancellationToken)
    {
        var flights = await _flightRepository.GetStaffFlightsAsync(request.Search, cancellationToken);
        return Result.Success(flights);
    }
}
