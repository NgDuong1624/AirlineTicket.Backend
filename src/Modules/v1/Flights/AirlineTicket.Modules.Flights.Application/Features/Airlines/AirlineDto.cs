using System;

namespace AirlineTicket.Modules.Flights.Application.Features.Airlines;

public record AirlineDto(
    Guid Id,
    string IataCode,
    string Name,
    string? LogoUrl,
    string? BaseCountry);
