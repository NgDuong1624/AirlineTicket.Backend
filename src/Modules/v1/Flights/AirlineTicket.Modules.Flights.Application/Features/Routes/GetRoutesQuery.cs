using MediatR;

namespace AirlineTicket.Modules.Flights.Application.Features.Routes;

public record GetRoutesQuery() : IRequest<object>;
