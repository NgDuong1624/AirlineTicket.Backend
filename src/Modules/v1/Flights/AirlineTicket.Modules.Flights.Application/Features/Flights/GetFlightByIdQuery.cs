using System;
using MediatR;

namespace AirlineTicket.Modules.Flights.Application.Features.Flights;

public record GetFlightByIdQuery(Guid Id) : IRequest<object>;
