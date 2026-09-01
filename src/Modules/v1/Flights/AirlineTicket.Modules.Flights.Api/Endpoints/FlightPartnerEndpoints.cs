using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Api.Extensions;
using AirlineTicket.Modules.Flights.Application.Features.Routes;
using AirlineTicket.Modules.Flights.Application.Features.Airplanes;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using AirlineTicket.Modules.Flights.Application.Features.Airlines;
using AirlineTicket.Modules.Flights.Application.Features.AircraftModels;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using AirlineTicket.BuildingBlocks.Domain.Constants;

namespace AirlineTicket.Modules.Flights.Api.Endpoints;

public class FlightPartnerEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // ——————————————————————— Partner Routes ————————————————————————————————
        var partnerRoutes = app.MapGroup("/api/partner/routes")
            .WithTags("Partner Routes")
            .RequireAuthorization(AuthConstants.Policies.PartnerOnly);

        // GET /api/partner/routes — Get paginated list of routes for partner
        partnerRoutes.MapGet("/", async (
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                [FromQuery] int? pageNumber,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10,
                CancellationToken ct = default) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var page = pageNumber ?? pageIndex;
                var result = await sender.Send(new GetRoutesByAirlineQuery(airlineId, page, pageSize), ct);
                return result.IsSuccess ? Results.Ok(new { Items = result.Items, TotalCount = result.TotalCount }) : Results.BadRequest(result.Error);
            })
            .WithSummary("Get paginated list of routes for partner")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // POST /api/partner/routes — Create a new route for partner
        partnerRoutes.MapPost("/", async (
                [FromBody] PartnerRouteRequest request,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var command = new CreateRouteCommand(airlineId, request.OriginAirportId, request.DestinationAirportId, request.DistanceKm, request.EstimatedDurationMinutes);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Created($"/api/partner/routes/{result.Value}", new { Id = result.Value }) : Results.BadRequest(result.Error);
            })
            .WithSummary("Create a new route for partner")
            .Produces(201)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // PUT /api/partner/routes/{id} — Update an existing route for partner
        partnerRoutes.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] PartnerRouteRequest request,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var command = new UpdateRouteCommand(id, airlineId, request.OriginAirportId, request.DestinationAirportId, request.DistanceKm, request.EstimatedDurationMinutes);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
            })
            .WithSummary("Update an existing route for partner")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // DELETE /api/partner/routes/{id} — Delete a route for partner
        partnerRoutes.MapDelete("/{id:guid}", async (
                Guid id,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var result = await sender.Send(new DeleteRouteCommand(id, airlineId), ct);
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            })
            .WithSummary("Delete a route for partner")
            .Produces(204)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // ——————————————————————— Partner Airplanes ————————————————————————————————
        var partnerAirplanes = app.MapGroup("/api/partner/airplanes")
            .WithTags("Partner Airplanes")
            .RequireAuthorization(AuthConstants.Policies.PartnerOnly);

        // GET /api/partner/airplanes — Get paginated list of airplanes for partner
        partnerAirplanes.MapGet("/", async (
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                [FromQuery] int? pageNumber,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10,
                CancellationToken ct = default) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var page = pageNumber ?? pageIndex;
                var result = await sender.Send(new GetAirplanesByAirlineQuery(airlineId, page, pageSize), ct);
                return result.IsSuccess ? Results.Ok(new { Items = result.Items, TotalCount = result.TotalCount }) : Results.BadRequest(result.Error);
            })
            .WithSummary("Get paginated list of airplanes for partner")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // GET /api/partner/airplanes/models — Get paginated list of aircraft models
        partnerAirplanes.MapGet("/models", async (
                [FromServices] ISender sender,
                CancellationToken ct,
                [FromQuery] int? pageNumber,
                [FromQuery] int? pageIndex = 1,
                [FromQuery] int? pageSize = 100) =>
            {
                var page = pageNumber ?? pageIndex;
                var result = await sender.Send(new GetAircraftModelsQuery(page, pageSize), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

        partnerAirplanes.MapPost("/", async (
                [FromBody] PartnerAirplaneRequest request,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var command = new CreateAirplaneCommand(airlineId, request.AircraftModelId, request.Model, request.RegistrationNumber, request.TotalCapacity);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Created($"/api/partner/airplanes/{result.Value}", new { Id = result.Value }) : Results.BadRequest(result.Error);
            });

        partnerAirplanes.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] PartnerAirplaneRequest request,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var command = new UpdateAirplaneCommand(id, airlineId, request.Model, request.RegistrationNumber, request.TotalCapacity);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
            });

        partnerAirplanes.MapDelete("/{id:guid}", async (
                Guid id,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var result = await sender.Send(new DeleteAirplaneCommand(id, airlineId), ct);
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            });

        // ——————————————————————— Partner Flights (stub — full scheduling CRUD pending) ————————————————————————————————
        var partnerFlights = app.MapGroup("/api/partner/flights")
            .WithTags("Partner Flights")
            .RequireAuthorization(AuthConstants.Policies.PartnerOnly);

        partnerFlights.MapGet("/", async (
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                [FromQuery] string? search,
                [FromQuery] int? status,
                [FromQuery] DateTime? departureDate,
                [FromQuery] int? pageNumber,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10,
                CancellationToken ct = default) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var page = pageNumber ?? pageIndex;
                var result = await sender.Send(new GetPartnerFlightsQuery(airlineId, page, pageSize, search, status, departureDate), ct);
                return result.IsSuccess ? Results.Ok(new { Items = result.Items, TotalCount = result.TotalCount }) : Results.BadRequest(result.Error);
            });

        partnerFlights.MapPost("/", async (
                [FromBody] PartnerFlightRequest request,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var command = new CreatePartnerFlightCommand(
                    airlineId,
                    request.RouteId,
                    request.AirplaneId,
                    request.FlightNumber,
                    request.BasePrice,
                    request.DepartureTime);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Created($"/api/partner/flights/{result.Value}", new { Id = result.Value }) : Results.BadRequest(result.Error);
            });

        partnerFlights.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] PartnerFlightRequest request,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var command = new UpdatePartnerFlightCommand(
                    id,
                    airlineId,
                    request.DepartureTime);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
            });

        partnerFlights.MapDelete("/{id:guid}", async (
                Guid id,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var result = await sender.Send(new DeletePartnerFlightCommand(id, airlineId), ct);
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            });

        // ——————————————————————— Partner Aircraft (stub — alias of airplanes view) ————————————————————————————————
        var partnerAircraft = app.MapGroup("/api/partner/aircraft")
            .WithTags("Partner Aircraft")
            .RequireAuthorization(AuthConstants.Policies.PartnerOnly);

        partnerAircraft.MapGet("/", async (
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                [FromQuery] int? pageNumber,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10,
                CancellationToken ct = default) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var page = pageNumber ?? pageIndex;
                var result = await sender.Send(new GetPartnerAircraftQuery(airlineId, page, pageSize), ct);
                return result.IsSuccess ? Results.Ok(new { Items = result.Items, TotalCount = result.TotalCount }) : Results.BadRequest(result.Error);
            });

        partnerAircraft.MapPost("/", async (
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var result = await sender.Send(new CreatePartnerAircraftCommand(airlineId), ct);
                return result.IsSuccess ? Results.Created($"/api/partner/aircraft/{result.Value}", new { Id = result.Value }) : Results.BadRequest(result.Error);
            });

        partnerAircraft.MapPut("/{id:guid}", async (
                Guid id,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var result = await sender.Send(new UpdatePartnerAircraftCommand(id, airlineId), ct);
                return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
            });

        partnerAircraft.MapDelete("/{id:guid}", async (
                Guid id,
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var result = await sender.Send(new DeletePartnerAircraftCommand(id, airlineId), ct);
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            });

        // ——————————————————————— Partner Settings (airline profile) ————————————————————————————————
        var partnerSettings = app.MapGroup("/api/partner/settings")
            .WithTags("Partner Settings")
            .RequireAuthorization(AuthConstants.Policies.PartnerOnly);

        partnerSettings.MapGet("/", async (
                ClaimsPrincipal principal,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var result = await sender.Send(new GetPartnerSettingsQuery(airlineId), ct);
                if (!result.IsSuccess) return Results.NotFound();
                var airline = result.Value;
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
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                if (!TryGetAirlineId(principal, out var airlineId)) return Forbidden();
                var command = new UpdatePartnerSettingsCommand(airlineId, request.AirlineName, request.Address, request.SupportEmail, request.SupportPhone);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
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
public sealed record PartnerAirplaneRequest(Guid? AircraftModelId, string Model, string RegistrationNumber, int TotalCapacity);
public sealed record PartnerSettingsRequest(string? AirlineName, string? Address, string? SupportEmail, string? SupportPhone);
public sealed record PartnerFlightRequest(Guid RouteId, Guid AirplaneId, string FlightNumber, decimal BasePrice, DateTime DepartureTime);
