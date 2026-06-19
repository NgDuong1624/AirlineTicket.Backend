using System;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using AirlineTicket.Modules.Flights.Domain.Entities;
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
                [FromServices] IAirportRepository repo,
                CancellationToken ct) =>
            {
                var items = await repo.GetAllAsync(ct);
                return Results.Ok(new { items, totalCount = items.Count });
            })
            .WithName("AdminGetAirports");

        adminAirports.MapPost("/", async (
                [FromBody] AdminAirportRequest request,
                [FromServices] IAirportRepository repo,
                CancellationToken ct) =>
            {
                var id = await repo.CreateAsync(new Airport
                {
                    IataCode = request.IataCode,
                    NameEn = request.NameEn,
                    NameVi = request.NameVi,
                    CityEn = request.CityEn,
                    CityVi = request.CityVi,
                    CountryCode = request.CountryCode,
                    Timezone = request.Timezone,
                    IsActive = request.IsActive ?? true
                }, ct);
                return Results.Created($"/api/admin/airports/{id}", new { Id = id });
            })
            .WithName("AdminCreateAirport");

        adminAirports.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] AdminAirportRequest request,
                [FromServices] IAirportRepository repo,
                CancellationToken ct) =>
            {
                await repo.UpdateAsync(new Airport
                {
                    Id = id,
                    IataCode = request.IataCode,
                    NameEn = request.NameEn,
                    NameVi = request.NameVi,
                    CityEn = request.CityEn,
                    CityVi = request.CityVi,
                    CountryCode = request.CountryCode,
                    Timezone = request.Timezone,
                    IsActive = request.IsActive ?? true
                }, ct);
                return Results.Ok();
            })
            .WithName("AdminUpdateAirport");

        adminAirports.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] IAirportRepository repo,
                CancellationToken ct) =>
            {
                await repo.DeleteAsync(id, ct);
                return Results.NoContent();
            })
            .WithName("AdminDeleteAirport");

        // ——————————————————————— Admin Airlines ————————————————————————————————
        var adminAirlines = app.MapGroup("/api/admin/airlines")
            .WithTags("Admin Airlines")
            .RequireAuthorization("AdminOnly");

        adminAirlines.MapGet("/", async (
                [FromServices] IAirlineRepository repo,
                CancellationToken ct) =>
            {
                var items = await repo.GetAllAsync(ct);
                return Results.Ok(new { items, totalCount = items.Count });
            })
            .WithName("AdminGetAirlines");

        adminAirlines.MapPost("/", async (
                [FromBody] AdminAirlineRequest request,
                [FromServices] IAirlineRepository repo,
                CancellationToken ct) =>
            {
                var id = await repo.CreateAsync(new Airline
                {
                    IataCode = request.IataCode,
                    Name = request.Name,
                    LogoUrl = request.LogoUrl,
                    BaseCountry = request.BaseCountry,
                    IsActive = request.IsActive ?? true
                }, ct);
                return Results.Created($"/api/admin/airlines/{id}", new { Id = id });
            })
            .WithName("AdminCreateAirline");

        adminAirlines.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] AdminAirlineRequest request,
                [FromServices] IAirlineRepository repo,
                CancellationToken ct) =>
            {
                await repo.UpdateAsync(new Airline
                {
                    Id = id,
                    IataCode = request.IataCode,
                    Name = request.Name,
                    LogoUrl = request.LogoUrl,
                    BaseCountry = request.BaseCountry,
                    IsActive = request.IsActive ?? true
                }, ct);
                return Results.Ok();
            })
            .WithName("AdminUpdateAirline");

        adminAirlines.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] IAirlineRepository repo,
                CancellationToken ct) =>
            {
                await repo.DeleteAsync(id, ct);
                return Results.NoContent();
            })
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
