using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Notifications.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Notifications.Application.BackgroundServices;

public sealed class NotificationProcessingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationProcessingBackgroundService> _logger;

    public NotificationProcessingBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationProcessingBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationProcessingBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

                var pending = await repository.GetPendingAsync(10);

                foreach (var notification in pending)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during notification processing.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
