using AirlineTicket.Modules.Bookings.Domain.Enums;
using AirlineTicket.Modules.Bookings.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Bookings.Infrastructure.BackgroundServices;

public class BookingRefundProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingRefundProcessor> _logger;

    public BookingRefundProcessor(IServiceScopeFactory scopeFactory, ILogger<BookingRefundProcessor> logger)
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
                    var cancelledBookings = await dbContext.Bookings
                        .Include(b => b.Payments)
                        .Where(b => b.Status == BookingStatus.Cancelled && b.Payments.Any(p => p.IsSuccessful && p.ProviderStatus != "Refunded"))
                        .ToListAsync(stoppingToken);

                    foreach (var booking in cancelledBookings)
                    {
                        foreach (var payment in booking.Payments.Where(p => p.IsSuccessful && p.ProviderStatus != "Refunded"))
                        {
                            // Mock refund logic
                            payment.ProviderStatus = "Refunded";
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing booking refunds.");
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
