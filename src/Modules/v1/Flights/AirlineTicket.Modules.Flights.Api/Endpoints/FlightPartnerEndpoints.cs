using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using RouteEntity = AirlineTicket.Modules.Flights.Domain.Entities.Route;

namespace AirlineTicket.Modules.Flights.Api.Endpoints;

public class FlightPartnerEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // ——————————————————————— Partner Routes ————————————————————————————————
        var partnerRoutes = app.MapGroup("/api/partner/routes")
            .WithTags("Partner Routes")
            .RequireAuthorization("PartnerOnly");

        partnerRoutes.MapGet("/", async (
                ClaimsPrincipal principal,
                [FromServices] IRouteRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var items = await repo.GetByAirlineAsync(airlineId, ct);
                return Results.Ok(new { items, totalCount = items.Count });
            });

        partnerRoutes.MapPost("/", async (
                [FromBody] PartnerRouteRequest request,
                ClaimsPrincipal principal,
                [FromServices] IRouteRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var id = await repo.CreateAsync(new RouteEntity
                {
                    AirlineId = airlineId,
                    OriginAirportId = request.OriginAirportId,
                    DestinationAirportId = request.DestinationAirportId,
                    DistanceKm = request.DistanceKm,
                    EstimatedDurationMinutes = request.EstimatedDurationMinutes
                }, ct);
                return Results.Created($"/api/partner/routes/{id}", new { Id = id });
            });

        partnerRoutes.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] PartnerRouteRequest request,
                ClaimsPrincipal principal,
                [FromServices] IRouteRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                await repo.UpdateAsync(new RouteEntity
                {
                    Id = id,
                    OriginAirportId = request.OriginAirportId,
                    DestinationAirportId = request.DestinationAirportId,
                    DistanceKm = request.DistanceKm,
                    EstimatedDurationMinutes = request.EstimatedDurationMinutes
                }, airlineId, ct);
                return Results.Ok();
            });

        partnerRoutes.MapDelete("/{id:guid}", async (
                Guid id,
                ClaimsPrincipal principal,
                [FromServices] IRouteRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                await repo.DeleteAsync(id, airlineId, ct);
                return Results.NoContent();
            });

        // ——————————————————————— Partner Airplanes ————————————————————————————————
        var partnerAirplanes = app.MapGroup("/api/partner/airplanes")
            .WithTags("Partner Airplanes")
            .RequireAuthorization("PartnerOnly");

        partnerAirplanes.MapGet("/", async (
                ClaimsPrincipal principal,
                [FromServices] IAirplaneRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var items = await repo.GetByAirlineAsync(airlineId, ct);
                return Results.Ok(new { items, totalCount = items.Count });
            });

        partnerAirplanes.MapPost("/", async (
                [FromBody] PartnerAirplaneRequest request,
                ClaimsPrincipal principal,
                [FromServices] IAirplaneRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var id = await repo.CreateAsync(new Airplane
                {
                    AirlineId = airlineId,
                    Model = request.Model,
                    RegistrationNumber = request.RegistrationNumber,
                    TotalCapacity = request.TotalCapacity
                }, ct);
                return Results.Created($"/api/partner/airplanes/{id}", new { Id = id });
            });

        partnerAirplanes.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] PartnerAirplaneRequest request,
                ClaimsPrincipal principal,
                [FromServices] IAirplaneRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                await repo.UpdateAsync(new Airplane
                {
                    Id = id,
                    Model = request.Model,
                    RegistrationNumber = request.RegistrationNumber,
                    TotalCapacity = request.TotalCapacity
                }, airlineId, ct);
                return Results.Ok();
            });

        partnerAirplanes.MapDelete("/{id:guid}", async (
                Guid id,
                ClaimsPrincipal principal,
                [FromServices] IAirplaneRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                await repo.DeleteAsync(id, airlineId, ct);
                return Results.NoContent();
            });

        // ——————————————————————— Partner Flights (stub — full scheduling CRUD pending) ————————————————————————————————
        var partnerFlights = app.MapGroup("/api/partner/flights")
            .WithTags("Partner Flights")
            .RequireAuthorization("PartnerOnly");

        partnerFlights.MapGet("/", () => Results.Ok(new { items = new List<object>(), totalCount = 0 }));
        partnerFlights.MapPost("/", () => Results.Created("/api/partner/flights/1", new { Id = Guid.NewGuid() }));
        partnerFlights.MapPut("/{id:guid}", (Guid id) => Results.Ok());
        partnerFlights.MapDelete("/{id:guid}", (Guid id) => Results.NoContent());

        // ——————————————————————— Partner Aircraft (stub — alias of airplanes view) ————————————————————————————————
        var partnerAircraft = app.MapGroup("/api/partner/aircraft")
            .WithTags("Partner Aircraft")
            .RequireAuthorization("PartnerOnly");

        partnerAircraft.MapGet("/", () => Results.Ok(new { items = new List<object>(), totalCount = 0 }));
        partnerAircraft.MapPost("/", () => Results.Created("/api/partner/aircraft/1", new { Id = Guid.NewGuid() }));
        partnerAircraft.MapPut("/{id:guid}", (Guid id) => Results.Ok());
        partnerAircraft.MapDelete("/{id:guid}", (Guid id) => Results.NoContent());

        // ——————————————————————— Partner Settings (airline profile) ————————————————————————————————
        var partnerSettings = app.MapGroup("/api/partner/settings")
            .WithTags("Partner Settings")
            .RequireAuthorization("PartnerOnly");

        partnerSettings.MapGet("/", async (
                ClaimsPrincipal principal,
                [FromServices] IAirlineRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var airline = await repo.GetByIdAsync(airlineId, ct);
                if (airline is null) return Results.NotFound();
                return Results.Ok(new
                {
                    airlineName = airline.Name,
                    address = airline.Address,
                    supportEmail = airline.SupportEmail,
                    supportPhone = airline.SupportPhone
                });
            });

        partnerSettings.MapPut("/", async (
                [FromBody] PartnerSettingsRequest request,
                ClaimsPrincipal principal,
                [FromServices] IAirlineRepository repo,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var airline = await repo.GetByIdAsync(airlineId, ct);
                if (airline is null) return Results.NotFound();

                if (!string.IsNullOrEmpty(request.AirlineName)) airline.Name = request.AirlineName;
                airline.Address = request.Address;
                airline.SupportEmail = request.SupportEmail;
                airline.SupportPhone = request.SupportPhone;
                await repo.UpdateAsync(airline, ct);
                return Results.Ok();
            });
    }

    private static bool TryGetAirlineId(ClaimsPrincipal principal, out Guid airlineId)
    {
        airlineId = Guid.Empty;
        return Guid.TryParse(principal.FindFirst("AirlineId")?.Value, out airlineId);
    }

    private static IResult Forbidden() =>
        Results.Json(new { Code = "FORBIDDEN", Message = "No airline scope on token." }, statusCode: 403);
}

public sealed record PartnerRouteRequest(Guid OriginAirportId, Guid DestinationAirportId, decimal? DistanceKm, int? EstimatedDurationMinutes);
public sealed record PartnerAirplaneRequest(string Model, string RegistrationNumber, int TotalCapacity);
public sealed record PartnerSettingsRequest(string? AirlineName, string? Address, string? SupportEmail, string? SupportPhone);
