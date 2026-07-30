using AirlineTicket.Modules.Logs.Infrastructure.BackgroundServices;
using AirlineTicket.BuildingBlocks.Logging;
using AirlineTicket.Modules.Logs.Application.Contracts;
using AirlineTicket.Modules.Logs.Infrastructure.Data;
using AirlineTicket.Modules.Logs.Infrastructure.Data.Repositories;
using AirlineTicket.Modules.Logs.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Modules.Logs.Infrastructure;

/// <summary>
/// Dependency injection extension methods for the Logs infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Logs infrastructure services into the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddLogsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LogsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

        services.AddScoped<ISystemLogService, SystemLogService>();
        services.AddScoped<ILogRepository, LogRepository>();

        return services;
    }

    public static IServiceCollection AddLogsBackgroundJobs(this IServiceCollection services)
    {
        services.AddHostedService<LogRetentionCleanerJob>();
        services.AddHostedService<DailySalesReportJob>();

        return services;
    }
}
