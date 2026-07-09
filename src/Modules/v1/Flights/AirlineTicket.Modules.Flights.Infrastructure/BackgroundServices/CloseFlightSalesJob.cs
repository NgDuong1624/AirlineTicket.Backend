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

public class CloseFlightSalesJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CloseFlightSalesJob> _logger;
    private readonly int _intervalSeconds;

    public CloseFlightSalesJob(
        IServiceScopeFactory scopeFactory,
        ILogger<CloseFlightSalesJob> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intervalSeconds = configuration.GetValue<int>("BackgroundJobs:CloseFlightSalesJob:IntervalSeconds", 300);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CloseFlightSalesJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
                var now = DateTime.UtcNow;
                var threshold = now.AddMinutes(60); // Close sales 60 mins before departure

                // Find flights that are within 60 mins of departure and haven't been closed yet
                // Assuming we might need a flag like IsSalesClosed, but for now we just log
                // or we could lock seats.
                
                _logger.LogDebug("CloseFlightSalesJob is running.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in CloseFlightSalesJob.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }
    }
}