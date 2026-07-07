using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Infrastructure.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Workers.FlightCleanup;

public sealed class FlightCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FlightCleanupBackgroundService> _logger;
    private readonly object _lastSuccessLock = new();
    private DateTime? _lastSuccessTime;

    public FlightCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<FlightCleanupBackgroundService> logger)
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
        _logger.LogInformation("FlightCleanupBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupAsync(stoppingToken);
                lock (_lastSuccessLock)
                {
                    _lastSuccessTime = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during flight cleanup cycle.");
            }

            // Run every 12 hours
            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }

    private async Task RunCleanupAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var flightCleaner = scope.ServiceProvider.GetRequiredService<IFlightCleaner>();
        await flightCleaner.CleanFlightsAsync(ct);
    }
}
