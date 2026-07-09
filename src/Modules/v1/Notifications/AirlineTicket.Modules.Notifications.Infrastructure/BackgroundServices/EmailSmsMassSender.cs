using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Notifications.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Notifications.Infrastructure.BackgroundServices;

public class EmailSmsMassSender : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailSmsMassSender> _logger;
    private readonly int _intervalSeconds;

    public EmailSmsMassSender(
        IServiceScopeFactory scopeFactory,
        ILogger<EmailSmsMassSender> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intervalSeconds = configuration.GetValue<int>("BackgroundJobs:EmailSmsMassSender:IntervalSeconds", 10);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailSmsMassSender started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

                // Fetch pending notifications (Email = 0, SMS = 1)
                var pending = await repository.GetPendingAsync(50);

                foreach (var notification in pending)
                {
                    if (notification.Type == 0 || notification.Type == 1)
                    {
                        var (success, error) = await sender.SendAsync(notification);
                        if (success)
                        {
                            await repository.MarkSentAsync(notification.Id);
                        }
                        else
                        {
                            await repository.MarkFailedAsync(notification.Id, error ?? "Unknown error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in EmailSmsMassSender.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }
    }
}