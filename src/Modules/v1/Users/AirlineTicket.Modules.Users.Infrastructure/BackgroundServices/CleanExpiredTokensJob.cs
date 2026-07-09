using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Users.Infrastructure.BackgroundServices;

public class CleanExpiredTokensJob(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<CleanExpiredTokensJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = config.GetValue("Users:TokenCleanupIntervalMinutes", 1440);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                logger.LogInformation("Expired tokens cleaned.");
            }
            catch (Exception ex) { logger.LogError(ex, "Error cleaning tokens."); }
            await Task.Delay(TimeSpan.FromMinutes(interval), stoppingToken);
        }
    }
}
