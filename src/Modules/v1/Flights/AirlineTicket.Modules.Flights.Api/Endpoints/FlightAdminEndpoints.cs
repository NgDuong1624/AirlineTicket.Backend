using System;
using System.Threading;
using System.ComponentModel.DataAnnotations;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using AirlineTicket.Modules.Flights.Application.Features.Airports;
using AirlineTicket.Modules.Flights.Application.Features.Airlines;
using AirlineTicket.Modules.Flights.Application.Features.AircraftModels;
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
                CancellationToken ct,
                [FromQuery] string? search,
                [FromQuery, Range(1, int.MaxValue)] int pageIndex = 1,
                [FromQuery, Range(1, 100)] int pageSize = 10) =>
            {
                var result = await sender.Send(new GetAirportsQuery(search, pageIndex, pageSize), ct);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
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
                CancellationToken ct,
                [FromQuery, Range(1, int.MaxValue)] int pageIndex = 1,
                [FromQuery, Range(1, 100)] int pageSize = 10) =>
            {
                var result = await sender.Send(new GetAirlinesQuery(pageIndex, pageSize), ct);
                return result.IsSuccess ? Results.Ok(new { Items = result.Value.Items, TotalCount = result.Value.TotalCount }) : Results.BadRequest(result.Error);
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
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10,
                CancellationToken ct = default) =>
            {
                var result = await sender.Send(new GetAdminFlightsQuery(pageIndex, pageSize), ct);
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

        // ——————————————————————— Admin Aircraft Models ————————————————————————————————
        var adminAircraftModels = app.MapGroup("/api/admin/aircraft-models")
            .WithTags("Admin Aircraft Models")
            .RequireAuthorization("AdminOnly");

        adminAircraftModels.MapGet("/", async (
                [FromQuery] int? pageIndex,
                [FromQuery] int? pageSize,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetAircraftModelsQuery(pageIndex, pageSize), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithName("AdminGetAircraftModels");

        adminAircraftModels.MapGet("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetAircraftModelByIdQuery(id), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
            })
            .WithName("AdminGetAircraftModelById");

        adminAircraftModels.MapPost("/", async (
                [FromBody] AdminAircraftModelRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new CreateAircraftModelCommand(
                    request.Name,
                    request.Manufacturer,
                    request.TotalSeats,
                    request.SeatTemplates);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Created($"/api/admin/aircraft-models/{result.Value}", new { Id = result.Value }) : Results.BadRequest(result.Error);
            })
            .WithName("AdminCreateAircraftModel");

        adminAircraftModels.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] AdminAircraftModelRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new UpdateAircraftModelCommand(
                    id,
                    request.Name,
                    request.Manufacturer,
                    request.TotalSeats,
                    request.SeatTemplates);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
            })
            .WithName("AdminUpdateAircraftModel");

        adminAircraftModels.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new DeleteAircraftModelCommand(id), ct);
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            })
            .WithName("AdminDeleteAircraftModel");
    }
}

public record AdminAircraftModelRequest(
    string Name,
    string Manufacturer,
    int TotalSeats,
    List<SeatTemplateDto> SeatTemplates);

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
