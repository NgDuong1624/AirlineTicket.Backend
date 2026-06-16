using System;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Flights.Api.Endpoints;

public class FlightPartnerEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var partnerRoutes = app.MapGroup("/api/partner/routes")
            .WithTags("Partner Routes")
            .RequireAuthorization("PartnerOnly");

        partnerRoutes.MapGet("/", () => Results.Ok(new List<object>()));
        partnerRoutes.MapPost("/", () => Results.Created("/api/partner/routes/1", new { Id = Guid.NewGuid() }));
        partnerRoutes.MapPut("/{id:guid}", (Guid id) => Results.Ok());
        partnerRoutes.MapDelete("/{id:guid}", (Guid id) => Results.NoContent());

        var partnerAirplanes = app.MapGroup("/api/partner/airplanes")
            .WithTags("Partner Airplanes")
            .RequireAuthorization("PartnerOnly");

        partnerAirplanes.MapGet("/", () => Results.Ok(new List<object>()));
        partnerAirplanes.MapPost("/", () => Results.Created("/api/partner/airplanes/1", new { Id = Guid.NewGuid() }));
        partnerAirplanes.MapPut("/{id:guid}", (Guid id) => Results.Ok());
        partnerAirplanes.MapDelete("/{id:guid}", (Guid id) => Results.NoContent());

        var partnerFlights = app.MapGroup("/api/partner/flights")
            .WithTags("Partner Flights")
            .RequireAuthorization("PartnerOnly");

        partnerFlights.MapGet("/", () => Results.Ok(new List<object>()));
        partnerFlights.MapPost("/", () => Results.Created("/api/partner/flights/1", new { Id = Guid.NewGuid() }));
        partnerFlights.MapPut("/{id:guid}", (Guid id) => Results.Ok());
        partnerFlights.MapDelete("/{id:guid}", (Guid id) => Results.NoContent());

        var partnerAircraft = app.MapGroup("/api/partner/aircraft")
            .WithTags("Partner Aircraft")
            .RequireAuthorization("PartnerOnly");

        partnerAircraft.MapGet("/", () => Results.Ok(new List<object>()));
        partnerAircraft.MapPost("/", () => Results.Created("/api/partner/aircraft/1", new { Id = Guid.NewGuid() }));
        partnerAircraft.MapPut("/{id:guid}", (Guid id) => Results.Ok());
        partnerAircraft.MapDelete("/{id:guid}", (Guid id) => Results.NoContent());
    }
}
