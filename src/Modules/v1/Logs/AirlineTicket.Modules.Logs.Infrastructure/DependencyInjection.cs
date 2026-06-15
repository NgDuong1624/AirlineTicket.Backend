using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Modules.Logs.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLogsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext when configured
        return services;
    }
}
