using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Flights.Infrastructure.Services;

public class SharedFareEvaluationService : ISharedFareEvaluationService
{
    private readonly FlightDbContext _context;

    public SharedFareEvaluationService(FlightDbContext context)
    {
        _context = context;
    }

    public async Task<LowestRoutePriceDto?> GetLowestFlightPriceForRouteDateAsync(
        Guid originAirportId,
        Guid destinationAirportId,
        DateOnly departureDate,
        CancellationToken cancellationToken = default)
    {
        var departureStart = departureDate.ToDateTime(TimeOnly.MinValue);
        var departureEnd = departureDate.ToDateTime(TimeOnly.MaxValue);

        var lowestFlight = await _context.Flights
            .Include(f => f.Route)
            .Where(f => !f.IsDeleted &&
                        f.Route.OriginAirportId == originAirportId &&
                        f.Route.DestinationAirportId == destinationAirportId &&
                        f.DepartureTime >= departureStart &&
                        f.DepartureTime <= departureEnd)
            .OrderBy(f => f.BasePrice)
            .Select(f => new LowestRoutePriceDto(
                f.Id,
                f.RouteId,
                f.Route.OriginAirportId,
                f.Route.DestinationAirportId,
                departureDate,
                f.BasePrice,
                f.Currency
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return lowestFlight;
    }

    public async Task RecordPriceHistorySnapshotAsync(
        Guid flightId,
        Guid routeId,
        decimal price,
        string seatClass = "Economy",
        CancellationToken cancellationToken = default)
    {
        var snapshot = new FlightPriceHistory
        {
            Id = Guid.NewGuid(),
            FlightId = flightId,
            RouteId = routeId,
            Price = price,
            SeatClass = seatClass,
            RecordedAt = DateTime.UtcNow
        };

        _context.FlightPriceHistories.Add(snapshot);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
