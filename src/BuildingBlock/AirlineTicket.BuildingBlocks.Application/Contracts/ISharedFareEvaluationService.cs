using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.BuildingBlocks.Application.Contracts;

public record LowestRoutePriceDto(
    Guid FlightId,
    Guid RouteId,
    Guid OriginAirportId,
    Guid DestinationAirportId,
    DateOnly DepartureDate,
    decimal LowestPrice,
    string Currency
);

/// <summary>
/// Service allowing cross-module route fare queries and price history recordings.
/// </summary>
public interface ISharedFareEvaluationService
{
    Task<LowestRoutePriceDto?> GetLowestFlightPriceForRouteDateAsync(
        Guid originAirportId,
        Guid destinationAirportId,
        DateOnly departureDate,
        CancellationToken cancellationToken = default);

    Task RecordPriceHistorySnapshotAsync(
        Guid flightId,
        Guid routeId,
        decimal price,
        string seatClass = "Economy",
        CancellationToken cancellationToken = default);
}
