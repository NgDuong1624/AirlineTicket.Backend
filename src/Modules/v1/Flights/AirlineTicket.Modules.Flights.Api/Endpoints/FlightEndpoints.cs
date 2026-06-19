using System;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Features.Airports;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using AirlineTicket.Modules.Flights.Application.Features.Routes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Flights.Api.Endpoints;

public class FlightEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // ——————————————————————— Airports ————————————————————————————————
        app.MapGroup("/api/airports")
            .WithTags("Airports Module")
                        .MapGet("/", async (
                    [FromQuery] string? search,
                    [FromServices] ISender sender,
                    CancellationToken ct) =>
                {
                    var query = new GetAirportsQuery(search);
                    var result = await sender.Send(query, ct);
                    return Results.Ok(result);
                })
            .WithName("GetAirports")
            .WithSummary("Lấy danh sách sân bay hoặc tìm kiếm theo từ khóa")
            .Produces(200)
            .AllowAnonymous();

        // ——————————————————————— Airlines ————————————————————————————————
        app.MapGroup("/api/airlines")
            .WithTags("Airlines Module")
            .MapGet("/", async (
                    [FromServices] IAirlineRepository airlineRepository,
                    CancellationToken ct) =>
                {
                    var result = await airlineRepository.GetAllAsync(ct);
                    return Results.Ok(new { airlines = result });
                })
            .WithName("GetAirlines")
            .WithSummary("Lấy danh sách hãng hàng không")
            .Produces(200)
            .AllowAnonymous();

        // ——————————————————————— Routes ————————————————————————————————
        app.MapGroup("/api/routes")
            .WithTags("Routes Module")
                        .MapGet("/", async (
                    [FromServices] ISender sender,
                    CancellationToken ct) =>
                {
                    var query = new GetRoutesQuery();
                    var result = await sender.Send(query, ct);
                    return Results.Ok(result);
                })
            .WithName("GetRoutes")
            .WithSummary("Lấy danh sách tuyến bay")
            .Produces(200)
            .AllowAnonymous();

        // ——————————————————————— Flights ————————————————————————————————
        var flightsGroup = app.MapGroup("/api/flights")
            .WithTags("Flights Module");

        // POST /api/flights — Search flights
        flightsGroup.MapPost("/", async (
                [FromBody] FlightSearchRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var query = new SearchFlightsQuery(
                        request.OriginCode,
                        request.DestinationCode,
                        request.DepartDate,
                        request.CabinClass,
                        request.Airlines,
                        request.PriceRangeMin,
                        request.PriceRangeMax,
                        request.MaxStops,
                        request.SortBy,
                        request.Currency);
                    var result = await sender.Send(query, ct);
                    return Results.Ok(result);
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.ValidationException ex)
                {
                    return Results.Json(new { Code = "VALIDATION_ERROR", Errors = ex.Errors }, statusCode: 400);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "INTERNAL_ERROR", Message = ex.Message }, statusCode: 500);
                }
            })
            .WithName("SearchFlights")
            .WithSummary("Tìm kiếm chuyến bay theo bộ lọc")
            .Produces(200)
            .Produces(400)
            .Produces(500)
            .AllowAnonymous();

        // GET /api/flights/{id}
        flightsGroup.MapGet("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetFlightByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result != null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetFlightById")
            .WithSummary("Lấy chi tiết chuyến bay theo ID")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();

        // GET /api/flights/trending
        flightsGroup.MapGet("/trending", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetTrendingFlightsQuery();
                var result = await sender.Send(query, ct);
                return Results.Ok(result);
            })
            .WithName("GetTrendingFlights")
            .WithSummary("Lấy danh sách các chuyến bay/tuyến đường phổ biến")
            .Produces(200)
            .AllowAnonymous();

        // GET /api/flights/{id}/seats
        flightsGroup.MapGet("/{id:guid}/seats", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetFlightSeatsQuery(id);
                var result = await sender.Send(query, ct);
                return result != null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetFlightSeats")
            .WithSummary("Lấy sơ đồ ghế của chuyến bay")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();

        // POST /api/flights (Admin create)
        flightsGroup.MapPost("/admin", async (
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
                    return Results.Created($"/api/flights/{result}", new { Id = result });
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.ValidationException ex)
                {
                    return Results.Json(new { Code = "VALIDATION_ERROR", Errors = ex.Errors }, statusCode: 400);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "INTERNAL_ERROR", Message = ex.Message }, statusCode: 500);
                }
            })
            .WithName("CreateFlight")
            .WithSummary("Tạo chuyến bay mới")
            .Produces(201)
            .Produces(400)
            .Produces(500)
            .RequireAuthorization("AdminOrStaff");
    }
}

// ======================= Requests =======================
public record FlightSearchRequest(
    string OriginCode,
    string DestinationCode,
    DateTime DepartDate,
    string? CabinClass = null,
    List<string>? Airlines = null,
    decimal? PriceRangeMin = null,
    decimal? PriceRangeMax = null,
    int? MaxStops = null,
    string? SortBy = null,
    string Currency = "VND");

public record CreateFlightRequest(
    Guid RouteId,
    Guid AirplaneId,
    string FlightNumber,
    decimal BasePrice,
    DateTime ScheduledDeparture,
    DateTime ScheduledArrival);