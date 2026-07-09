using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Domain.Enums;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Flights.Infrastructure.BackgroundServices;

public class FlightDelayDetectorJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FlightDelayDetectorJob> _logger;
    private readonly int _intervalSeconds;

    public FlightDelayDetectorJob(
        IServiceScopeFactory scopeFactory,
        ILogger<FlightDelayDetectorJob> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intervalSeconds = configuration.GetValue<int>("BackgroundJobs:FlightDelayDetectorJob:IntervalSeconds", 300);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FlightDelayDetectorJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
                var now = DateTime.UtcNow;

                // Detect flights that should have departed but haven't
                var delayedFlights = await context.Flights
                    .Where(f => !f.IsDeleted && 
                                (f.Status == FlightStatus.Scheduled || f.Status == FlightStatus.Boarding) && 
                                f.DepartureTime < now)
                    .ToListAsync(stoppingToken);

                if (delayedFlights.Any())
                {
                    foreach (var flight in delayedFlights)
                    {
                        flight.Status = FlightStatus.Delayed;
                        flight.UpdatedAt = now;
                        // TODO: Trigger event via Message Queue
                    }

                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Marked {Count} flights as Delayed.", delayedFlights.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in FlightDelayDetectorJob.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }
    }
}