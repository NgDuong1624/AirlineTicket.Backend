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

public class CheckInReminderJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CheckInReminderJob> _logger;
    private readonly int _intervalSeconds;

    public CheckInReminderJob(
        IServiceScopeFactory scopeFactory,
        ILogger<CheckInReminderJob> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intervalSeconds = configuration.GetValue<int>("BackgroundJobs:CheckInReminderJob:IntervalSeconds", 3600); // Run hourly
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CheckInReminderJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
                var now = DateTime.UtcNow;
                var targetTime = now.AddHours(24);

                // Find flights departing in exactly 24 hours (within a 1-hour window)
                var flights = await context.Flights
                    .Where(f => !f.IsDeleted && 
                                f.Status == FlightStatus.Scheduled && 
                                f.DepartureTime > now && 
                                f.DepartureTime <= targetTime)
                    .ToListAsync(stoppingToken);

                if (flights.Any())
                {
                    _logger.LogInformation("Found {Count} flights for check-in reminders.", flights.Count);
                    // TODO: Trigger notification events for passengers
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in CheckInReminderJob.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }
    }
}