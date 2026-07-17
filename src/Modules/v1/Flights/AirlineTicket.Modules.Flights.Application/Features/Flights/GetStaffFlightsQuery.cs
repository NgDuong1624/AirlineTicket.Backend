using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

using AirlineTicket.BuildingBlocks.Auth;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetStaffFlightsQuery(string? Search = null) : IQuery<Result<List<StaffFlightListItemDto>>>;

public class GetStaffFlightsQueryHandler : IQueryHandler<GetStaffFlightsQuery, Result<List<StaffFlightListItemDto>>>
{
    private readonly IFlightRepository _flightRepository;
    private readonly ICurrentUser _currentUser;

    public GetStaffFlightsQueryHandler(IFlightRepository flightRepository, ICurrentUser currentUser)
    {
        _flightRepository = flightRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<List<StaffFlightListItemDto>>> Handle(GetStaffFlightsQuery request, CancellationToken cancellationToken)
    {
        var airlineId = _currentUser.IsAdmin ? (Guid?)null : _currentUser.AirlineId;
        var flights = await _flightRepository.GetStaffFlightsAsync(request.Search, airlineId, cancellationToken);
        return Result.Success(flights);
    }
}
