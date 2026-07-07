using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Domain.Enums;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Flights.Infrastructure.BackgroundServices;

public interface IFlightCleaner
{
    Task CleanFlightsAsync(CancellationToken ct);
}

public class FlightCleaner : IFlightCleaner
{
    private readonly FlightDbContext _context;
    private readonly ILogger<FlightCleaner> _logger;

    public FlightCleaner(FlightDbContext context, ILogger<FlightCleaner> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CleanFlightsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // 1. Auto-land flights whose arrival time has passed
        var flightsToLand = await _context.Flights
            .Where(f => !f.IsDeleted 
                && f.ArrivalTime < now 
                && f.Status != FlightStatus.Landed 
                && f.Status != FlightStatus.Cancelled)
            .ToListAsync(ct);

        if (flightsToLand.Any())
        {
            _logger.LogInformation("Found {Count} flights to mark as Landed.", flightsToLand.Count);
            foreach (var flight in flightsToLand)
            {
                flight.Status = FlightStatus.Landed;
                flight.UpdatedAt = now;
            }
        }

        if (flightsToLand.Any())
        {
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Flight cleanup completed successfully.");
        }
        else
        {
            _logger.LogInformation("No flights required cleanup.");
        }
    }
}
