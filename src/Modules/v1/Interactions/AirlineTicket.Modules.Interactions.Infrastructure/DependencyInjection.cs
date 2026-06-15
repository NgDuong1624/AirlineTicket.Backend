using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Modules.Interactions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInteractionsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext when configured
        return services;
    }
}
