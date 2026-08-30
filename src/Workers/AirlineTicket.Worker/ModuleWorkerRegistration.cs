using System.Reflection;
using AirlineTicket.Api.Services;
using AirlineTicket.BuildingBlocks.Api.Auth;
using AirlineTicket.BuildingBlocks.Infrastructure;
using AirlineTicket.Modules.Bookings.Application;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Infrastructure;
using AirlineTicket.Modules.CMS.Application;
using AirlineTicket.Modules.CMS.Infrastructure;
using AirlineTicket.Modules.Flights.Application;
using AirlineTicket.Modules.Flights.Infrastructure;
using AirlineTicket.Modules.Interactions.Application;
using AirlineTicket.Modules.Interactions.Infrastructure;
using AirlineTicket.Modules.Logs.Application;
using AirlineTicket.Modules.Logs.Infrastructure;
using AirlineTicket.Modules.Notifications.Application;
using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Infrastructure;
using AirlineTicket.Modules.Promotions.Application;
using AirlineTicket.Modules.Promotions.Infrastructure;
using AirlineTicket.Modules.Users.Application;
using AirlineTicket.Modules.Users.Infrastructure;
using AirlineTicket.SignalR.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Worker;

public static class ModuleWorkerRegistration
{
    public static IServiceCollection AddAllModuleWorkers(this IServiceCollection services, IConfiguration configuration)
    {
        // Distributed cache fallback
        services.AddDistributedMemoryCache();

        // Building blocks (Logging, Caching, Correlation)
        services.AddBuildingBlocksInfrastructure();
        services.AddBuildingBlocksAuth();

        // SignalR & Pusher
        services.AddSignalR();
        services.AddScoped<INotificationPusher, NotificationPusher>();

        // Register each module's infrastructure (DbContexts, repositories)
        services.AddBookingsInfrastructure(configuration);
        services.AddFlightsInfrastructure(configuration);
        services.AddNotificationsInfrastructure(configuration);
        services.AddPromotionsInfrastructure(configuration);
        services.AddUsersInfrastructure(configuration);
        services.AddInteractionsInfrastructure(configuration);
        services.AddLogsInfrastructure(configuration);
        services.AddCMSInfrastructure(configuration);

        // Cross-module service implementations
        services.AddScoped<IFlightSeatReservation, FlightSeatReservation>();
        services.AddScoped<IStaffSalesReader, StaffSalesReader>();

        // MediatR registration for services requiring ISender / IPublisher
        var applicationAssemblies = new Assembly[]
        {
            typeof(FlightSeatReservation).Assembly,
            typeof(BookingsApplicationMarker).Assembly,
            typeof(FlightsApplicationMarker).Assembly,
            typeof(PromotionsApplicationMarker).Assembly,
            typeof(UsersApplicationMarker).Assembly,
            typeof(InteractionsApplicationMarker).Assembly,
            typeof(CMSApplicationMarker).Assembly,
            typeof(NotificationsApplicationMarker).Assembly,
            typeof(LogsApplicationMarker).Assembly,
        };

        services.AddMediatR(cfg =>
        {
            var licenseKey = configuration["LuckyPenny:MediatR:LicenseKey"];
            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                cfg.LicenseKey = licenseKey;
            }
            cfg.RegisterServicesFromAssemblies(applicationAssemblies);
        });

        // Register each module's background jobs
        services.AddBookingsBackgroundJobs();
        services.AddFlightsBackgroundJobs();
        services.AddNotificationsBackgroundJobs();
        services.AddPromotionsBackgroundJobs();
        services.AddUsersBackgroundJobs();
        services.AddInteractionsBackgroundJobs();
        services.AddLogsBackgroundJobs();

        return services;
    }
}
