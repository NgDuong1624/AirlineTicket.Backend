using System;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Flights.Api.Endpoints;

public class FlightAdminEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // ——————————————————————— Admin Airports ————————————————————————————————
        var adminAirports = app.MapGroup("/api/admin/airports")
            .WithTags("Admin Airports")
            .RequireAuthorization("AdminOnly");

        adminAirports.MapGet("/", () => Results.Ok(new List<object>()))
            .WithName("AdminGetAirports");
            
        adminAirports.MapPost("/", () => Results.Created("/api/admin/airports/1", new { Id = Guid.NewGuid() }))
            .WithName("AdminCreateAirport");

        adminAirports.MapPut("/{id:guid}", (Guid id) => Results.Ok())
            .WithName("AdminUpdateAirport");

        adminAirports.MapDelete("/{id:guid}", (Guid id) => Results.NoContent())
            .WithName("AdminDeleteAirport");

        // ——————————————————————— Admin Airlines ————————————————————————————————
        var adminAirlines = app.MapGroup("/api/admin/airlines")
            .WithTags("Admin Airlines")
            .RequireAuthorization("AdminOnly");

        adminAirlines.MapGet("/", () => Results.Ok(new List<object>()))
            .WithName("AdminGetAirlines");

        adminAirlines.MapPost("/", () => Results.Created("/api/admin/airlines/1", new { Id = Guid.NewGuid() }))
            .WithName("AdminCreateAirline");

        adminAirlines.MapPut("/{id:guid}", (Guid id) => Results.Ok())
            .WithName("AdminUpdateAirline");

        adminAirlines.MapDelete("/{id:guid}", (Guid id) => Results.NoContent())
            .WithName("AdminDeleteAirline");

        // ——————————————————————— Admin Flights ————————————————————————————————
        var adminFlights = app.MapGroup("/api/admin/flights")
            .WithTags("Admin Flights")
            .RequireAuthorization("AdminOnly");

        adminFlights.MapGet("/", () => Results.Ok(new List<object>()))
            .WithName("AdminGetFlights");

        adminFlights.MapPost("/", async (
                [FromBody] CreateFlightRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var command = new CreateFlightCommand(
                        request.RouteId,
                        request.AirplaneId,
                        request.FlightNumber,
                        request.BasePrice,
                        request.ScheduledDeparture,
                        request.ScheduledArrival);

                    var result = await sender.Send(command, ct);
                    return Results.Created($"/api/admin/flights/{result}", new { Id = result });
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "INTERNAL_ERROR", Message = ex.Message }, statusCode: 500);
                }
            })
            .WithName("AdminCreateFlight");

        adminFlights.MapPut("/{id:guid}", (Guid id) => Results.Ok())
            .WithName("AdminUpdateFlight");

        adminFlights.MapDelete("/{id:guid}", (Guid id) => Results.NoContent())
            .WithName("AdminDeleteFlight");
    }
}
