using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

using AirlineTicket.BuildingBlocks.Auth;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetStaffFlightsQuery(string? Search = null, int PageIndex = 1, int PageSize = 10) : IQuery<Result<PagedResult<StaffFlightListItemDto>>>;

public class GetStaffFlightsQueryHandler : IQueryHandler<GetStaffFlightsQuery, Result<PagedResult<StaffFlightListItemDto>>>
{
    private readonly IFlightRepository _flightRepository;
    private readonly ICurrentUser _currentUser;

    public GetStaffFlightsQueryHandler(IFlightRepository flightRepository, ICurrentUser currentUser)
    {
        _flightRepository = flightRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<StaffFlightListItemDto>>> Handle(GetStaffFlightsQuery request, CancellationToken cancellationToken)
    {
        var airlineId = _currentUser.IsAdmin ? (Guid?)null : _currentUser.AirlineId;
        var (items, totalCount) = await _flightRepository.GetStaffFlightsAsync(request.Search, airlineId, request.PageIndex, request.PageSize, cancellationToken);
        return Result.Success(PagedResult<StaffFlightListItemDto>.Success(items, request.PageIndex, request.PageSize, totalCount));
    }
}
