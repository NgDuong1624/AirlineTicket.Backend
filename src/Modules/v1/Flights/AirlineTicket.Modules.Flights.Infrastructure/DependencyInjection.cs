using AirlineTicket.BuildingBlocks.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Infrastructure.BackgroundServices;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using AirlineTicket.Modules.Flights.Infrastructure.Data.Repositories;
using AirlineTicket.Modules.Flights.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Modules.Flights.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFlightsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FlightDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure())
                .UseSnakeCaseNamingConvention());

        // Register repositories
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<IAirportRepository, AirportRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IAirplaneRepository, AirplaneRepository>();
        services.AddScoped<IFlightSeatRepository, FlightSeatRepository>();
        services.AddScoped<IAirlineRepository, AirlineRepository>();
        services.AddScoped<IAircraftModelRepository, AircraftModelRepository>();

        // Register cross-module shared services
        services.AddScoped<ISharedFlightSearchService, SharedFlightSearchService>();
        services.AddScoped<ISharedFareEvaluationService, SharedFareEvaluationService>();
        services.AddScoped<ISeatGenerationService, SeatGenerationService>();

        // Register flight generator (scoped, consumed by the background worker)
        services.AddScoped<IFlightGenerator, FlightGenerator>();
        services.AddScoped<IFlightCleaner, FlightCleaner>();

        return services;
    }

    public static IServiceCollection AddFlightsBackgroundJobs(this IServiceCollection services)
    {
        services.AddHostedService<FlightStatusAutomatorJob>();
        services.AddHostedService<FlightDelayDetectorJob>();
        services.AddHostedService<CloseFlightSalesJob>();
        services.AddHostedService<CheckInReminderJob>();
        services.AddHostedService<DynamicPricingJob>();
        services.AddHostedService<FlightCleanupBackgroundService>();
        services.AddHostedService<FlightGenerationBackgroundService>();

        return services;
    }
}
