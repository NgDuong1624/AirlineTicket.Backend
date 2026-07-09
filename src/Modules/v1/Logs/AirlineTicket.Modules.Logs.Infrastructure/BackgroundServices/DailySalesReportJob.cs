using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Logs.Infrastructure.BackgroundServices;

public class DailySalesReportJob(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<DailySalesReportJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var triggerHour = config.GetValue("Logs:DailySalesReportHour", 2);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nextRun = DateTime.Today.AddDays(1).AddHours(triggerHour);
                var delay = nextRun - DateTime.Now;
                if (delay < TimeSpan.Zero) delay = TimeSpan.FromDays(1);

                using var scope = scopeFactory.CreateScope();
                logger.LogInformation("Daily sales report generated.");
            }
            catch (Exception ex) { logger.LogError(ex, "Error generating daily sales report."); }
            await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
        }
    }
}
