using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record CreateFlightCommand(Guid RouteId, Guid AirplaneId, string FlightNumber, decimal BasePrice, DateTime ScheduledDeparture, DateTime ScheduledArrival) : IRequest<Guid>;

public class CreateFlightCommandHandler : IRequestHandler<CreateFlightCommand, Guid>
{
    private readonly IFlightRepository _flightRepository;
    
    public CreateFlightCommandHandler(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }
    
    public async Task<Guid> Handle(CreateFlightCommand request, CancellationToken cancellationToken)
    {
        var flight = new FlightDto
        {
            Id = Guid.NewGuid(),
            RouteId = request.RouteId,
            AirplaneId = request.AirplaneId,
            FlightNumber = request.FlightNumber,
            BasePrice = request.BasePrice
        };
        
        return await _flightRepository.CreateAsync(flight, cancellationToken);
    }
}
