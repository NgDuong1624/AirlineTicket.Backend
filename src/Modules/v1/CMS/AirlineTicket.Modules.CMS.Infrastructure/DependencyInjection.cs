using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Modules.CMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCMSInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext when configured
        return services;
    }
}
