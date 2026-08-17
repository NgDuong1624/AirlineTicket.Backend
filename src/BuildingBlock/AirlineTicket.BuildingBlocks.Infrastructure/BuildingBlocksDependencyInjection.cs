using AirlineTicket.BuildingBlocks.Application.Localization;
using AirlineTicket.BuildingBlocks.Caching;
using AirlineTicket.BuildingBlocks.Infrastructure.Caching;
using AirlineTicket.BuildingBlocks.Infrastructure.Localization;
using AirlineTicket.BuildingBlocks.Infrastructure.Logging;
using AirlineTicket.BuildingBlocks.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.BuildingBlocks.Infrastructure;

/// <summary>
/// Registers cross-cutting BuildingBlocks services for logging, caching, correlation, and i18n localization.
/// Centralized DI so modules and the host don't need to know implementation details.
/// Future migration: swap any registration here without touching module code.
/// </summary>
public static class BuildingBlocksDependencyInjection
{
    /// <summary>
    /// Adds all BuildingBlocks cross-cutting services (logging, caching, correlation, localization).
    /// </summary>
    public static IServiceCollection AddBuildingBlocksInfrastructure(this IServiceCollection services)
    {
        // ---------------------------------------------------------------
        // HttpContextAccessor
        // ---------------------------------------------------------------
        services.AddHttpContextAccessor();

        // ---------------------------------------------------------------
        // Logging & Correlation
        // ---------------------------------------------------------------
        services.AddSingleton<CorrelationContext>();
        services.AddSingleton<ICorrelationContext>(sp => sp.GetRequiredService<CorrelationContext>());
        services.AddScoped<ILoggingService, LoggingService>();

        // ---------------------------------------------------------------
        // Caching — uses IDistributedCache (Redis in production / Memory in dev)
        // ---------------------------------------------------------------
        services.AddScoped<ICacheService, DistributedCacheService>();

        // ---------------------------------------------------------------
        // i18n Error Localization
        // ---------------------------------------------------------------
        services.AddSingleton<ILanguageResolver, HeaderLanguageResolver>();
        services.AddSingleton<IErrorLocalizer, JsonErrorLocalizer>();

        return services;
    }
}
