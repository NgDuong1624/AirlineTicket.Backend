using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Flights.Infrastructure.BackgroundServices;

public interface IFlightGenerator
{
    Task GenerateFlightsAsync(DateTime targetDate, CancellationToken ct);
}

public class FlightGenerator : IFlightGenerator
{
    private readonly FlightDbContext _context;
    private readonly ILogger<FlightGenerator> _logger;

    public FlightGenerator(FlightDbContext context, ILogger<FlightGenerator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task GenerateFlightsAsync(DateTime targetDate, CancellationToken ct)
    {
        // Check if any flight already exists for this date
        var hasFlights = await _context.Flights
            .AnyAsync(f => !f.IsDeleted
                && f.DepartureTime.Year == targetDate.Year
                && f.DepartureTime.Month == targetDate.Month
                && f.DepartureTime.Day == targetDate.Day, ct);

        if (hasFlights)
        {
            _logger.LogInformation("Date {Date}: flights already exist, skipping.", targetDate.ToString("yyyy-MM-dd"));
            return;
        }

        _logger.LogInformation("Date {Date}: no flights found, generating...", targetDate.ToString("yyyy-MM-dd"));

        // Load all active routes with their related data
        var routes = await _context.Routes
            .Where(r => !r.IsDeleted)
            .Include(r => r.Airline)
            .Include(r => r.OriginAirport)
            .Include(r => r.DestinationAirport)
            .ToListAsync(ct);

        // Designate route(s) to receive multiple daily flights (first route, or any with specific criteria)
        var multiFlightRouteIds = routes
            .Take(1) // At least one route gets 2+ flights per day
            .Select(r => r.Id)
            .ToHashSet();

        foreach (var route in routes)
        {
            // Pick the first active airplane for this airline
            var airplane = await _context.Airplanes
                .Where(a => a.AirlineId == route.AirlineId && !a.IsDeleted)
                .FirstOrDefaultAsync(ct);

            if (airplane is null)
            {
                _logger.LogWarning("Route {RouteId}: no available airplane for airline {AirlineId}, skipping.",
                    route.Id, route.AirlineId);
                continue;
            }

            // Determine how many flights to generate for this route
            var isMultiFlight = multiFlightRouteIds.Contains(route.Id);
            var departureHours = isMultiFlight
                ? new[] { 8, 16 }  // 08:00 and 16:00 for multi-flight routes
                : new[] { 8 };      // 08:00 for standard routes

            var durationMinutes = route.EstimatedDurationMinutes ?? 120;
            var airlineCode = route.Airline?.IataCode ?? "XX";

            foreach (var hour in departureHours)
            {
                var departureTime = targetDate.AddHours(hour);
                var arrivalTime = departureTime.AddMinutes(durationMinutes);

                // Unique flight number per route + hour
                var routeFlightCount = await _context.Flights
                    .CountAsync(f => f.RouteId == route.Id && !f.IsDeleted, ct);

                var flightNumber = $"{airlineCode}{routeFlightCount + 100:D3}";

                var basePrice = 50m + (decimal)(route.DistanceKm ?? 500) * 0.1m;
                basePrice = Math.Round(basePrice, 0);

                var flight = new Flight
                {
                    Id = Guid.NewGuid(),
                    RouteId = route.Id,
                    AirplaneId = airplane.Id,
                    FlightNumber = flightNumber,
                    BasePrice = basePrice,
                    DepartureTime = departureTime,
                    ArrivalTime = arrivalTime,
                    Status = Domain.Enums.FlightStatus.Scheduled,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Flights.Add(flight);

                // Seed FlightSeats from the airplane's seat templates
                var templateSeats = await _context.AirplaneSeats
                    .AsNoTracking()
                    .Where(s => s.AirplaneId == airplane.Id)
                    .ToListAsync(ct);

                foreach (var ts in templateSeats)
                {
                    var multiplier = ts.PriceMultiplier <= 0 ? 1.0m : ts.PriceMultiplier;
                    _context.FlightSeats.Add(new FlightSeat
                    {
                        Id = Guid.NewGuid(),
                        FlightId = flight.Id,
                        SeatNumber = ts.SeatNumber,
                        SeatClass = ts.SeatClass,
                        PriceOverride = multiplier == 1.0m ? null : decimal.Round(flight.BasePrice * multiplier, 2),
                        IsAvailable = true,
                        IsExtraLegroom = ts.IsExtraLegroom
                    });
                }

                _logger.LogDebug("Generated flight {FlightNumber} for route {RouteId} on {Date} at {Hour}:00.",
                    flightNumber, route.Id, targetDate.ToString("yyyy-MM-dd"), hour);
            }
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Date {Date}: generated flights for {RouteCount} routes.",
            targetDate.ToString("yyyy-MM-dd"), routes.Count);
    }
}
