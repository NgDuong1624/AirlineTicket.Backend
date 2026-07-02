using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

            // Determine flight times
            var durationMinutes = route.EstimatedDurationMinutes ?? 120;
            var departureTime = targetDate.AddHours(8); // Default 8:00 AM
            var arrivalTime = departureTime.AddMinutes(durationMinutes);

            // Build a flight number from airline IataCode + route sequential number
            var airlineCode = route.Airline?.IataCode ?? "XX";

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

            _logger.LogDebug("Generated flight {FlightNumber} for route {RouteId} on {Date}.",
                flightNumber, route.Id, targetDate.ToString("yyyy-MM-dd"));
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Date {Date}: generated flights for {RouteCount} routes.",
            targetDate.ToString("yyyy-MM-dd"), routes.Count);
    }
}

public sealed class FlightGenerationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FlightGenerationBackgroundService> _logger;
    private readonly object _lastSuccessLock = new();
    private DateTime? _lastSuccessTime;

    public FlightGenerationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<FlightGenerationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public DateTime? GetLastSuccessTime()
    {
        lock (_lastSuccessLock) { return _lastSuccessTime; }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FlightGenerationBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GenerateFlightsForNext14DaysAsync(stoppingToken);
                lock (_lastSuccessLock)
                {
                    _lastSuccessTime = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during flight generation cycle.");
            }

            // Wait 24 hours before the next cycle
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task GenerateFlightsForNext14DaysAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var flightGenerator = scope.ServiceProvider.GetRequiredService<IFlightGenerator>();

        var today = DateTime.UtcNow.Date;

        for (var dayOffset = 0; dayOffset < 14; dayOffset++)
        {
            ct.ThrowIfCancellationRequested();
            var targetDate = today.AddDays(dayOffset);
            await flightGenerator.GenerateFlightsAsync(targetDate, ct);
        }
    }
}
