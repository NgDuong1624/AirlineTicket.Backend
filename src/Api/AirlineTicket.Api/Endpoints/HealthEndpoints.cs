using AirlineTicket.Modules.Flights.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AirlineTicket.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health/live", () =>
        {
            return Results.Ok(new
            {
                status = "Healthy",
                server = "Running",
                timestamp = DateTime.UtcNow
            });
        })
        .WithName("GetHealthLiveness")
        .WithTags("Health");

        app.MapGet("/api/health", async (FlightDbContext dbContext) =>
        {
            var database = "Unknown";
            var status = "Healthy";

            try
            {
                var canConnect = await dbContext.Database.CanConnectAsync();
                database = canConnect ? "Connected" : "Disconnected";
                if (!canConnect) status = "Degraded";
            }
            catch
            {
                status = "Degraded";
                database = "Unreachable";
            }

            var result = new
            {
                status,
                server = "Running",
                database,
                timestamp = DateTime.UtcNow
            };

            return Results.Ok(result);
        })
        .WithName("GetHealth")
        .WithTags("Health");
    }
}
