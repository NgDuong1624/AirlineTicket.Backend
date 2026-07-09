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

public class DynamicPricingJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DynamicPricingJob> _logger;
    private readonly int _intervalSeconds;

    public DynamicPricingJob(
        IServiceScopeFactory scopeFactory,
        ILogger<DynamicPricingJob> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intervalSeconds = configuration.GetValue<int>("BackgroundJobs:DynamicPricingJob:IntervalSeconds", 3600); // Run hourly
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DynamicPricingJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
                
                // Logic to adjust prices based on load factor
                _logger.LogDebug("DynamicPricingJob is running.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DynamicPricingJob.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }
    }
}