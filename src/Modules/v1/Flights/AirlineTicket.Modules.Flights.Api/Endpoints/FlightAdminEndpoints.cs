using System;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using AirlineTicket.Modules.Flights.Application.Features.Airports;
using AirlineTicket.Modules.Flights.Application.Features.Airlines;
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

        adminAirports.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetAirportsQuery(null), ct);
                return result.IsSuccess ? Results.Ok(new { Items = result.Value, TotalCount = result.Value.Count }) : Results.BadRequest(result.Error);
            })
            .WithName("AdminGetAirports");

        adminAirports.MapPost("/", async (
                [FromBody] AdminAirportRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new CreateAirportCommand(
                    request.IataCode,
                    request.NameEn,
                    request.NameVi,
                    request.CityEn,
                    request.CityVi,
                    request.CountryCode,
                    request.Timezone,
                    request.IsActive);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Created($"/api/admin/airports/{result.Value}", new { Id = result.Value }) : Results.BadRequest(result.Error);
            })
            .WithName("AdminCreateAirport");

        adminAirports.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] AdminAirportRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new UpdateAirportCommand(
                    id,
                    request.IataCode,
                    request.NameEn,
                    request.NameVi,
                    request.CityEn,
                    request.CityVi,
                    request.CountryCode,
                    request.Timezone,
                    request.IsActive);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
            })
            .WithName("AdminUpdateAirport");

        adminAirports.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new DeleteAirportCommand(id), ct);
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            })
            .WithName("AdminDeleteAirport");

        // ——————————————————————— Admin Airlines ————————————————————————————————
        var adminAirlines = app.MapGroup("/api/admin/airlines")
            .WithTags("Admin Airlines")
            .RequireAuthorization("AdminOnly");

        adminAirlines.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetAirlinesQuery(), ct);
                return result.IsSuccess ? Results.Ok(new { Items = result.Value, TotalCount = result.Value.Count }) : Results.BadRequest(result.Error);
            })
            .WithName("AdminGetAirlines");

        adminAirlines.MapPost("/", async (
                [FromBody] AdminAirlineRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new CreateAirlineCommand(
                    request.IataCode,
                    request.Name,
                    request.LogoUrl,
                    request.BaseCountry,
                    request.IsActive);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Created($"/api/admin/airlines/{result.Value}", new { Id = result.Value }) : Results.BadRequest(result.Error);
            })
            .WithName("AdminCreateAirline");

        adminAirlines.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] AdminAirlineRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new UpdateAirlineCommand(
                    id,
                    request.IataCode,
                    request.Name,
                    request.LogoUrl,
                    request.BaseCountry,
                    request.IsActive);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
            })
            .WithName("AdminUpdateAirline");

        adminAirlines.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new DeleteAirlineCommand(id), ct);
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            })
            .WithName("AdminDeleteAirline");

        // ——————————————————————— Admin Flights ————————————————————————————————
        var adminFlights = app.MapGroup("/api/admin/flights")
            .WithTags("Admin Flights")
            .RequireAuthorization("AdminOnly");

        adminFlights.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetAdminFlightsQuery(), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithName("AdminGetFlights");

        adminFlights.MapPost("/", async (
                [FromBody] CreateFlightRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new CreateFlightCommand(
                    request.RouteId,
                    request.AirplaneId,
                    request.FlightNumber,
                    request.BasePrice,
                    request.ScheduledDeparture,
                    request.ScheduledArrival);

                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Created($"/api/admin/flights/{result.Value}", new { Id = result.Value }) : Results.BadRequest(result.Error);
            })
            .WithName("AdminCreateFlight");

        adminFlights.MapPut("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new UpdateFlightCommand(id), ct);
                return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
            })
            .WithName("AdminUpdateFlight");

        adminFlights.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new DeleteFlightCommand(id), ct);
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            })
            .WithName("AdminDeleteFlight");
    }
}

public sealed record AdminAirlineRequest(
    string IataCode,
    string Name,
    string? LogoUrl,
    string? BaseCountry,
    bool? IsActive);

public sealed record AdminAirportRequest(
    string IataCode,
    string NameEn,
    string NameVi,
    string CityEn,
    string CityVi,
    string CountryCode,
    string Timezone,
    bool? IsActive);
