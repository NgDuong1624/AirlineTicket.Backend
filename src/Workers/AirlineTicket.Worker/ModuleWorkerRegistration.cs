using AirlineTicket.Modules.Bookings.Infrastructure;
using AirlineTicket.Modules.Flights.Infrastructure;
using AirlineTicket.Modules.Notifications.Infrastructure;
using AirlineTicket.Modules.Promotions.Infrastructure;
using AirlineTicket.Modules.Users.Infrastructure;
using AirlineTicket.Modules.Interactions.Infrastructure;
using AirlineTicket.Modules.Logs.Infrastructure;
using AirlineTicket.Modules.CMS.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Worker;

public static class ModuleWorkerRegistration
{
    public static IServiceCollection AddAllModuleWorkers(this IServiceCollection services, IConfiguration configuration)
    {
        // Register each module's infrastructure (which registers DbContexts, repositories, and background services)
        services.AddBookingsInfrastructure(configuration);
        services.AddFlightsInfrastructure(configuration);
        services.AddNotificationsInfrastructure(configuration);
        services.AddPromotionsInfrastructure(configuration);
        services.AddUsersInfrastructure(configuration);
        services.AddInteractionsInfrastructure(configuration);
        services.AddLogsInfrastructure(configuration);
        services.AddCMSInfrastructure(configuration);

        return services;
    }
}
