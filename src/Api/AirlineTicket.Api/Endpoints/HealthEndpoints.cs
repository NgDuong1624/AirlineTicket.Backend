using AirlineTicket.Modules.Flights.Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health/flight-gen", (FlightGenerationBackgroundService service) =>
        {
            var lastSuccess = service.GetLastSuccessTime();
            return lastSuccess.HasValue
                ? Results.Ok(new { status = "Healthy", lastSuccess = lastSuccess.Value })
                : Results.Ok(new { status = "Running (No success yet)", lastSuccess = (DateTime?)null });
        })
        .WithName("GetFlightGenHealth")
        .WithTags("Health")
        .RequireAuthorization(); // Secured endpoint
    }
}
