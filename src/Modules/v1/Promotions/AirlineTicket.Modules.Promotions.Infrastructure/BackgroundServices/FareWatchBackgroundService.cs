using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Application.Contracts;
using AirlineTicket.Modules.Promotions.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Promotions.Infrastructure.BackgroundServices;

public class FareWatchBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FareWatchBackgroundService> _logger;
    private readonly int _intervalMinutes;

    public FareWatchBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<FareWatchBackgroundService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intervalMinutes = configuration.GetValue<int>("BackgroundJobs:FareWatchJob:IntervalMinutes", 15);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FareWatchBackgroundService started with interval {Interval}m.", _intervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessFareAlertsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in FareWatchBackgroundService.");
            }

            await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
        }
    }

    private async Task ProcessFareAlertsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var promotionContext = scope.ServiceProvider.GetRequiredService<PromotionDbContext>();
        var sharedFareService = scope.ServiceProvider.GetRequiredService<ISharedFareEvaluationService>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeAlerts = await promotionContext.FareAlerts
            .Where(a => a.IsActive && a.DepartureDate >= today)
            .ToListAsync(cancellationToken);

        if (!activeAlerts.Any())
            return;

        // Group by distinct route-date pairs to batch flight queries efficiently
        var routeDateGroups = activeAlerts
            .GroupBy(a => new { a.OriginAirportId, a.DestinationAirportId, a.DepartureDate })
            .ToList();

        foreach (var group in routeDateGroups)
        {
            var lowestFlightInfo = await sharedFareService.GetLowestFlightPriceForRouteDateAsync(
                group.Key.OriginAirportId,
                group.Key.DestinationAirportId,
                group.Key.DepartureDate,
                cancellationToken);

            if (lowestFlightInfo == null)
                continue;

            var currentLowestPrice = lowestFlightInfo.LowestPrice;

            // Ingest historical price snapshot into flights module via shared contract
            await sharedFareService.RecordPriceHistorySnapshotAsync(
                lowestFlightInfo.FlightId,
                lowestFlightInfo.RouteId,
                currentLowestPrice,
                "Economy",
                cancellationToken);

            // Evaluate individual alerts in group
            foreach (var alert in group)
            {
                alert.CurrentLowestPrice = currentLowestPrice;
                alert.LastCheckedAt = DateTime.UtcNow;

                var shouldNotify = currentLowestPrice <= alert.TargetPrice &&
                    (alert.LastNotifiedAt == null ||
                     alert.LastNotifiedAt < DateTime.UtcNow.AddHours(-24) ||
                     (alert.LastNotifiedPrice.HasValue && currentLowestPrice < alert.LastNotifiedPrice.Value));

                if (shouldNotify)
                {
                    alert.LastNotifiedAt = DateTime.UtcNow;
                    alert.LastNotifiedPrice = currentLowestPrice;

                    _logger.LogInformation(
                        "Price drop notification triggered for Alert {AlertId}: User {UserId}, Target {Target}, New Lowest {Price}",
                        alert.Id, alert.UserId, alert.TargetPrice, currentLowestPrice);
                }
            }
        }

        await promotionContext.SaveChangesAsync(cancellationToken);
    }
}
