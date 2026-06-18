using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

/// <summary>Flight list for the staff portal, with route, schedule and seat-availability summary.</summary>
public record GetStaffFlightsQuery(string? Search = null) : IRequest<List<StaffFlightListItemDto>>;

public class GetStaffFlightsQueryHandler : IRequestHandler<GetStaffFlightsQuery, List<StaffFlightListItemDto>>
{
    private readonly IFlightRepository _flightRepository;

    public GetStaffFlightsQueryHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public async Task<List<StaffFlightListItemDto>> Handle(GetStaffFlightsQuery request, CancellationToken cancellationToken)
    {
        return await _flightRepository.GetStaffFlightsAsync(request.Search, cancellationToken);
    }
}
