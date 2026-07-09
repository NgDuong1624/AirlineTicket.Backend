using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Promotions.Infrastructure.BackgroundServices;

public class PromotionStatusUpdaterJob(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<PromotionStatusUpdaterJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = config.GetValue("Promotions:IntervalMinutes", 60);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                logger.LogInformation("Promotion status updated.");
            }
            catch (Exception ex) { logger.LogError(ex, "Error updating promotions."); }
            await Task.Delay(TimeSpan.FromMinutes(interval), stoppingToken);
        }
    }
}
