using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.BuildingBlocks.Endpoints;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly[] assemblies)
    {
        var endpointTypes = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var type in endpointTypes)
        {
            services.AddTransient(typeof(IEndpoint), type);
        }

        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        
        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}
