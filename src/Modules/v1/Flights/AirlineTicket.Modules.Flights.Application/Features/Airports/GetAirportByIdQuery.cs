using System;
using MediatR;

namespace AirlineTicket.Modules.Flights.Application.Features.Airports;

public record GetAirportByIdQuery(Guid Id) : IRequest<object>;
