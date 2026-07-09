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

public class FlightStatusAutomatorJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FlightStatusAutomatorJob> _logger;
    private readonly int _intervalSeconds;

    public FlightStatusAutomatorJob(
        IServiceScopeFactory scopeFactory,
        ILogger<FlightStatusAutomatorJob> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intervalSeconds = configuration.GetValue<int>("BackgroundJobs:FlightStatusAutomatorJob:IntervalSeconds", 60);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FlightStatusAutomatorJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
                var now = DateTime.UtcNow;

                // Scheduled -> Boarding (40 mins before departure)
                var boardingThreshold = now.AddMinutes(40);
                var toBoarding = await context.Flights
                    .Where(f => !f.IsDeleted && f.Status == FlightStatus.Scheduled && f.DepartureTime <= boardingThreshold)
                    .ToListAsync(stoppingToken);

                foreach (var flight in toBoarding)
                {
                    flight.Status = FlightStatus.Boarding;
                    flight.UpdatedAt = now;
                }

                // Boarding -> InAir (at departure time)
                var toInAir = await context.Flights
                    .Where(f => !f.IsDeleted && f.Status == FlightStatus.Boarding && f.DepartureTime <= now)
                    .ToListAsync(stoppingToken);

                foreach (var flight in toInAir)
                {
                    flight.Status = FlightStatus.InAir;
                    flight.UpdatedAt = now;
                }

                // InAir -> Landed (at arrival time)
                var toLanded = await context.Flights
                    .Where(f => !f.IsDeleted && f.Status == FlightStatus.InAir && f.ArrivalTime <= now)
                    .ToListAsync(stoppingToken);

                foreach (var flight in toLanded)
                {
                    flight.Status = FlightStatus.Landed;
                    flight.UpdatedAt = now;
                }

                if (toBoarding.Any() || toInAir.Any() || toLanded.Any())
                {
                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Updated statuses: {Boarding} to Boarding, {InAir} to InAir, {Landed} to Landed.", 
                        toBoarding.Count, toInAir.Count, toLanded.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in FlightStatusAutomatorJob.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }
    }
}