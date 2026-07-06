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
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

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
        services.AddScoped<ISeatGenerationService, SeatGenerationService>();

        // Register background services
        services.AddScoped<IFlightGenerator, FlightGenerator>();
        services.AddSingleton<FlightGenerationBackgroundService>();
        services.AddHostedService(sp => sp.GetRequiredService<FlightGenerationBackgroundService>());

        return services;
    }
}
