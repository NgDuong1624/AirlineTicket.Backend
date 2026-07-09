using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Interactions.Infrastructure.BackgroundServices;

public class ChatbotTimeoutHandler(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<ChatbotTimeoutHandler> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = config.GetValue("Interactions:ChatbotTimeoutIntervalMinutes", 5);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                logger.LogInformation("Chatbot timeouts handled.");
            }
            catch (Exception ex) { logger.LogError(ex, "Error handling chatbot timeouts."); }
            await Task.Delay(TimeSpan.FromMinutes(interval), stoppingToken);
        }
    }
}
