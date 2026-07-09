using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Flights.Infrastructure.BackgroundServices;

public sealed class FlightCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FlightCleanupBackgroundService> _logger;

    public FlightCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<FlightCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FlightCleanupBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var flightCleaner = scope.ServiceProvider.GetRequiredService<IFlightCleaner>();
                await flightCleaner.CleanFlightsAsync(stoppingToken);

                _logger.LogInformation("Flight cleanup cycle completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during flight cleanup cycle.");
            }

            // Run every 12 hours
            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}
