using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Notifications.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Notifications.Infrastructure.BackgroundServices;

public class FlightDelayNotifierJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FlightDelayNotifierJob> _logger;
    private readonly int _intervalSeconds;

    public FlightDelayNotifierJob(
        IServiceScopeFactory scopeFactory,
        ILogger<FlightDelayNotifierJob> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intervalSeconds = configuration.GetValue<int>("BackgroundJobs:FlightDelayNotifierJob:IntervalSeconds", 60);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FlightDelayNotifierJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                // Logic to scan delayed flights and notify passengers
                // This would typically involve querying a message queue or a specific table
                // For now, we just log that it's running
                _logger.LogDebug("FlightDelayNotifierJob is running.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in FlightDelayNotifierJob.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }
    }
}