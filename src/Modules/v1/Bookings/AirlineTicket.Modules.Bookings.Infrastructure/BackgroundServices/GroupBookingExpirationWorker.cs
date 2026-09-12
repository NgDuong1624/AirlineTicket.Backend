using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Bookings.Infrastructure.BackgroundServices;

public class GroupBookingExpirationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GroupBookingExpirationWorker> _logger;

    public GroupBookingExpirationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<GroupBookingExpirationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IGroupBookingRepository>();
                var seatReservation = scope.ServiceProvider.GetRequiredService<IFlightSeatReservation>();

                var expiredGroups = await repository.GetExpiredActiveGroupsAsync(DateTime.UtcNow, stoppingToken);

                foreach (var group in expiredGroups)
                {
                    _logger.LogInformation("Expiring group booking {GroupId} ({InviteCode})", group.Id, group.InviteCode);

                    var outboundSeats = group.Members
                        .Where(m => !string.IsNullOrWhiteSpace(m.SeatNumber))
                        .Select(m => m.SeatNumber!)
                        .ToList();

                    if (outboundSeats.Count > 0)
                    {
                        try
                        {
                            await seatReservation.ReleaseSeatsAsync(group.FlightId, outboundSeats, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to release outbound seats for expired group {GroupId}", group.Id);
                        }
                    }

                    if (group.ReturnFlightId.HasValue)
                    {
                        var returnSeats = group.Members
                            .Where(m => !string.IsNullOrWhiteSpace(m.ReturnSeatNumber))
                            .Select(m => m.ReturnSeatNumber!)
                            .ToList();

                        if (returnSeats.Count > 0)
                        {
                            try
                            {
                                await seatReservation.ReleaseSeatsAsync(group.ReturnFlightId.Value, returnSeats, stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to release return seats for expired group {GroupId}", group.Id);
                            }
                        }
                    }

                    group.MarkExpired();
                    await repository.UpdateAsync(group, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing group booking expirations");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
