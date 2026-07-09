using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Logs.Infrastructure.BackgroundServices;

public class LogRetentionCleanerJob(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<LogRetentionCleanerJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = config.GetValue("Logs:CleanupIntervalHours", 24);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                logger.LogInformation("Old logs cleaned.");
            }
            catch (Exception ex) { logger.LogError(ex, "Error cleaning logs."); }
            await Task.Delay(TimeSpan.FromHours(interval), stoppingToken);
        }
    }
}
