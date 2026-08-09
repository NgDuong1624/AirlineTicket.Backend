using AirlineTicket.Modules.Bookings.Domain.Enums;
using AirlineTicket.Modules.Bookings.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Bookings.Infrastructure.BackgroundServices;

public class CancelExpiredBookingsJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CancelExpiredBookingsJob> _logger;

    public CancelExpiredBookingsJob(IServiceScopeFactory scopeFactory, ILogger<CancelExpiredBookingsJob> logger)
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
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
                    var expiredBookings = await dbContext.Bookings
                        .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt < DateTime.UtcNow.AddMinutes(-15))
                        .ToListAsync(stoppingToken);

                    foreach (var booking in expiredBookings)
                    {
                        booking.Status = BookingStatus.Cancelled;
                        // TODO: Seat release logic
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cancelling expired bookings.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
