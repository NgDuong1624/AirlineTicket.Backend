using AirlineTicket.Modules.Flights.Infrastructure.BackgroundServices;
using Microsoft.Extensions.Options;

namespace AirlineTicket.Workers.FlightGeneration;

public sealed class FlightGenerationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FlightGenerationBackgroundService> _logger;
    private readonly IOptions<FlightGenerationOptions> _options;
    private readonly object _lastSuccessLock = new();
    private DateTime? _lastSuccessTime;

    public FlightGenerationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<FlightGenerationBackgroundService> logger,
        IOptions<FlightGenerationOptions>? options = null)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options ?? Microsoft.Extensions.Options.Options.Create<FlightGenerationOptions>(new());
    }

    public DateTime? GetLastSuccessTime()
    {
        lock (_lastSuccessLock) { return _lastSuccessTime; }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FlightGenerationBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GenerateFlightsForWindowAsync(stoppingToken);
                lock (_lastSuccessLock)
                {
                    _lastSuccessTime = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during flight generation cycle.");
            }

            // Wait 24 hours before the next cycle
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task GenerateFlightsForWindowAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var flightGenerator = scope.ServiceProvider.GetRequiredService<IFlightGenerator>();

        var today = DateTime.UtcNow.Date;
        var windowDays = Math.Clamp(_options.Value.GenerationWindowDays, 1, 14);

        for (var dayOffset = 0; dayOffset < windowDays; dayOffset++)
        {
            ct.ThrowIfCancellationRequested();
            var targetDate = today.AddDays(dayOffset);
            await flightGenerator.GenerateFlightsAsync(targetDate, ct);
        }
    }
}

public class FlightGenerationOptions
{
    /// <summary>Number of days ahead to generate flights for. Max 14.</summary>
    public int GenerationWindowDays { get; set; } = 14;
}
