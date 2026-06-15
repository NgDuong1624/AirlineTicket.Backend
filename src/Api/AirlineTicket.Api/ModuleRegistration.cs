using System.Reflection;
using AirlineTicket.Modules.Bookings.Application;
using AirlineTicket.Modules.Flights.Application;
using AirlineTicket.Modules.Promotions.Application;
using AirlineTicket.Modules.Users.Application;
using AirlineTicket.Modules.Interactions.Application;
using AirlineTicket.Modules.CMS.Application;
using AirlineTicket.Modules.Notifications.Application;
using AirlineTicket.Modules.Logs.Application;

namespace AirlineTicket.Api;

public static class ModuleRegistration
{
    /// <summary>
    /// Danh sách tất cả Application Assemblies từ các modules
    /// </summary>
    public static Assembly[] ApplicationAssemblies { get; } =
    [
        typeof(BookingsApplicationMarker).Assembly,
        typeof(FlightsApplicationMarker).Assembly,
        typeof(PromotionsApplicationMarker).Assembly,
        typeof(UsersApplicationMarker).Assembly,
        typeof(InteractionsApplicationMarker).Assembly,
        typeof(CMSApplicationMarker).Assembly,
        typeof(NotificationsApplicationMarker).Assembly,
        typeof(LogsApplicationMarker).Assembly,
    ];
}
