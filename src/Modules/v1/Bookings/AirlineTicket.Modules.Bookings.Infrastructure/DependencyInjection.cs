using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Infrastructure.BackgroundServices;
using AirlineTicket.Modules.Bookings.Infrastructure.Data;
using AirlineTicket.Modules.Bookings.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Modules.Bookings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BookingDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));
        
        // Register repositories
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IRevenueRepository, RevenueRepository>();
        
        return services;
    }

    public static IServiceCollection AddBookingsBackgroundJobs(this IServiceCollection services)
    {
        services.AddHostedService<CancelExpiredBookingsJob>();
        services.AddHostedService<BookingRefundProcessor>();
        
        return services;
    }
}
