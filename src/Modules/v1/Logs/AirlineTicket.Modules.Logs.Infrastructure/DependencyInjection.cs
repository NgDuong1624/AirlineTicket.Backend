using AirlineTicket.Modules.Logs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Modules.Logs.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLogsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LogsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));
        
        return services;
    }
}
