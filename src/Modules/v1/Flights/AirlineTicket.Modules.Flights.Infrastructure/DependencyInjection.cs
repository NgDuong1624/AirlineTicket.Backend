using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using AirlineTicket.Modules.Flights.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Modules.Flights.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFlightsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FlightDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        // Register repositories
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<IAirportRepository, AirportRepository>();
        
        return services;
    }
}
