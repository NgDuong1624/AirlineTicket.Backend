using System;
using MediatR;

namespace AirlineTicket.Modules.Flights.Application.Features.Airports;

public record GetAirportsQuery(string? Search) : IRequest<object>;
