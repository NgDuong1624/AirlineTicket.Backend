using System;
using MediatR;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetFlightSeatsQuery(Guid FlightId) : IRequest<object>;
